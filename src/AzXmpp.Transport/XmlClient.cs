using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AzXmpp.Edge;
using AzXmpp.Transport.Sockets;
using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Transport
{
    /// <summary>
    /// Represents a client that reads XML.
    /// </summary>
    internal class XmlClient : IDisposable, IXmlConnectionEvents
    {
        private struct StanzaWriteRequest
        {
            public readonly Stanza Stanza;
            public readonly TaskCompletionSource<int> CompletionSource;

            public StanzaWriteRequest(Stanza stanza)
            {
                Stanza = stanza;
                CompletionSource = new TaskCompletionSource<int>();
            }
        }

        private readonly ISocket _socket;
        private readonly CancellationToken _cancellationToken;
        private Stream _stream;
        private IXmlConnection _client;

        private XmlReader _reader;
        private XmlWriter _writer;

        private XContainer _current;

        private readonly ConcurrentQueue<StanzaWriteRequest> _pendingStanzas;
        private volatile TaskCompletionSource<int> _stanzaReady;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlClient" /> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        public XmlClient(ISocket socket, CancellationToken cancellationToken)
        {
            _socket = socket;
            _cancellationToken = cancellationToken;
            _stream = new SocketStream(socket);
            _pendingStanzas = new ConcurrentQueue<StanzaWriteRequest>();
        }

        /// <summary>
        /// Asynchronously creates the XML reader.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous create reader operation.
        /// </returns>
        private Task CreateXmlAsync()
        {
            _current = new XDocument();

            if (_reader != null) _reader.Dispose();
            if (_writer != null) _writer.Dispose();

            _reader = XmlReader.Create(_stream, new XmlReaderSettings()
            {
                Async = true,
                CheckCharacters = true,
                CloseInput = false,
                ConformanceLevel = ConformanceLevel.Document,
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true
            });

            _writer = XmlWriter.Create(_stream, new XmlWriterSettings()
            {
                Async = true,
                CheckCharacters = true,
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Document,
                Indent = false,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false,
                WriteEndDocumentOnClose = false,
                Encoding = new UTF8Encoding(false)
            });

            return _client.OnStreamResetAsync();
        }

        /// <summary>
        /// Accepts the connection.
        /// </summary>
        public async void Open()
        {
            using (_socket)
            {
                Write();

                try
                {
                    _client = ActorProxy.Create<IXmlConnection>(ActorId.NewId(), Constants.ApplicationName, Constants.ActorNames.UnboundClient);
                    await _client.SubscribeAsync(this);
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ClientConnectionFailure(_socket.Identifier, e.ToString());
                    return;
                }

                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await CreateXmlAsync();

                        var element = await ReadStartElementAsync(_cancellationToken);
                        var stanza = Stanza.FromOpeningTag(element);

                        while (!_cancellationToken.IsCancellationRequested)
                        {
                            var result = await _client.OnStanzaReceivedAsync(stanza);

                            // Remove stanza elements from the document once we are
                            // done with them.
                            if (!stanza.OpeningTagOnly) stanza.Element.Remove();

                            if (result != null)
                            {
                                if (result.Response != null)
                                {
                                    await WriteStanzaAsync(result.Response);
                                }

                                switch (result.StreamAction)
                                {
                                    case StreamAction.Close: await CloseAsync(); return;
                                    case StreamAction.Abort: return;
                                    case StreamAction.StartTls: await StartTlsAsync(); break;
                                }

                                if ((int)result.StreamAction >= (int)StreamAction.Reset) break;
                            }

                            element = await ReadFullElementAsync(_cancellationToken);
                            stanza = Stanza.FromFullElement(element);
                        }
                    }
                    catch (Exception e)
                    {
                        var result = await _client.OnErrorOccurredAsync(e);
                        if (result != null && result.Response != null)
                        {
                            await WriteStanzaAsync(result.Response);
                        }

                        // Errors at this level always terminate the connection.
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the specified stanza to the connection.
        /// </summary>
        /// <param name="stanza">The stanza.</param>
        public async void WriteStanza(Stanza stanza)
        {
            try
            {
                await WriteStanzaAsync(stanza);
            }
            catch (Exception e)
            {
                var tmp = _client.OnErrorOccurredAsync(e);
            }
        }

        /// <summary>
        /// Asynchronously writes the specified stanza to the stream.
        /// </summary>
        /// <param name="stanza">The stanza.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write stanza operation.
        /// </returns>
        private Task WriteStanzaAsync(Stanza stanza)
        {
            var request = new StanzaWriteRequest(stanza);
            _pendingStanzas.Enqueue(request);
            Interlocked.Exchange(ref _stanzaReady, new TaskCompletionSource<int>()).TrySetResult(0);
            return request.CompletionSource.Task;
        }

        /// <summary>
        /// Continuously writes stanzas to the stream.
        /// </summary>
        private async void Write()
        {
            StanzaWriteRequest request;

            try
            {
                _stanzaReady = new TaskCompletionSource<int>();
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await _stanzaReady.Task;
                        _stanzaReady = new TaskCompletionSource<int>();
                    }
                    catch
                    {
                        break;
                    }

                    while (!_cancellationToken.IsCancellationRequested && _pendingStanzas.TryDequeue(out request))
                    {
                        using (_cancellationToken.Register(() => request.CompletionSource.TrySetCanceled()))
                        {
                            if (request.Stanza != null)
                            {
                                await request.Stanza.Element.WriteToAsync(_writer, !request.Stanza.OpeningTagOnly);
                            }

                            // Force a state transition to text. This will
                            // ensure that the closing '>' is written.
                            await _writer.WriteStringAsync("");
                            await _writer.FlushAsync();
                            request.CompletionSource.TrySetResult(0);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                while (_pendingStanzas.TryDequeue(out request))
                {
                    request.CompletionSource.TrySetCanceled();
                }
            }
            catch (OperationCanceledException)
            {
                while (_pendingStanzas.TryDequeue(out request))
                {
                    request.CompletionSource.TrySetCanceled();
                }
            }
            catch (Exception e)
            {
                while (_pendingStanzas.TryDequeue(out request))
                {
                    request.CompletionSource.TrySetException(e);
                }
            }
        }

        /// <summary>
        /// Asynchronously writes pending data and closes the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous close operation.
        /// </returns>
        public async Task CloseAsync()
        {
            await WriteStanzaAsync(null);
            await _writer.WriteEndDocumentAsync();
            await _writer.FlushAsync();
        }

        /// <summary>
        /// Asynchronously starts a TLS session on the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous start TLS operation.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task StartTlsAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously reads a opening tag from the underlying stream.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task{XElement}"/> that represents the asynchronous read start element operation.
        /// </returns>
        private async Task<XElement> ReadStartElementAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && await _reader.ReadAsync())
            {
                var isEmpty = _reader.IsEmptyElement; // attribute methods clear this.
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        var elem = new XElement(XName.Get(_reader.LocalName, _reader.NamespaceURI));
                        while (_reader.MoveToNextAttribute())
                        {
                            if (_reader.LocalName != "xmlns")
                                elem.Add(new XAttribute(XName.Get(_reader.LocalName, _reader.NamespaceURI), _reader.Value));
                        }
                        _current.Add(elem);
                        _current = elem;
                        if (isEmpty) goto case XmlNodeType.EndElement;
                        return elem;
                    case XmlNodeType.Text:
                        _current.Add(new XText(_reader.Value));
                        break;
                    case XmlNodeType.CDATA:
                        _current.Add(new XCData(_reader.Value));
                        break;
                    case XmlNodeType.Comment:
                        _current.Add(new XComment(_reader.Value));
                        break;
                    case XmlNodeType.EndElement:
                        var current = _current;
                        if (current.NodeType == XmlNodeType.Document)
                            return null;
                        _current = current.Parent;
                        if (_reader.NodeType == XmlNodeType.Element)
                            return (XElement)current;
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Asynchronously reads an entire element from the underlying stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task{XElement}"/> that represents the pending read operation.
        /// </returns>
        protected virtual async Task<XElement> ReadFullElementAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var start = _current;

            while (!cancellationToken.IsCancellationRequested && await _reader.ReadAsync())
            {
                var isEmpty = _reader.IsEmptyElement;
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        var elem = new XElement(XName.Get(_reader.LocalName, _reader.NamespaceURI));
                        while (_reader.MoveToNextAttribute())
                        {
                            if (_reader.LocalName != "xmlns")
                                elem.Add(new XAttribute(XName.Get(_reader.LocalName, _reader.NamespaceURI), _reader.Value));
                        }
                        _current.Add(elem);
                        _current = elem;
                        if (isEmpty) goto case XmlNodeType.EndElement;
                        break;
                    case XmlNodeType.Text:
                        _current.Add(new XText(_reader.Value));
                        break;
                    case XmlNodeType.CDATA:
                        _current.Add(new XCData(_reader.Value));
                        break;
                    case XmlNodeType.Comment:
                        _current.Add(new XComment(_reader.Value));
                        break;
                    case XmlNodeType.EndElement:
                        var current = _current;
                        _current = current.Parent;
                        if (_current == start) return (XElement)current;
                        break;
                }
            }

            return null;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
