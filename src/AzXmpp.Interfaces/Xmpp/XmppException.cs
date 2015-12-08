using System;
using System.Runtime.Serialization;

namespace AzXmpp.Xmpp
{

    /// <summary>
    /// Represents an exception about an XMPP connection.
    /// </summary>
    [Serializable]
    public class XmppException : Exception
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public XmppErrorCode ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        public XmppException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public XmppException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public XmppException(string message, Exception inner)
            : base(message ?? inner?.Message, inner)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public XmppException(XmppErrorCode errorCode)
            : base(DefaultMessage(errorCode))
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public XmppException(XmppErrorCode errorCode, string message)
            : base(DefaultMessage(errorCode, message))
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException" /> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="inner">The inner exception.</param>
        public XmppException(XmppErrorCode errorCode, Exception inner)
            : base(DefaultMessage(errorCode, inner?.Message))
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public XmppException(XmppErrorCode errorCode, string message, Exception inner)
            : base(DefaultMessage(errorCode, message ?? inner?.Message), inner)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmppException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected XmppException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            ErrorCode = (XmppErrorCode)info.GetInt32("ErrorCode");
        }

        /// <summary>
        /// Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", (int)ErrorCode);
        }

        /// <summary>
        /// Creates an error message for the specified error code if one is not
        /// specified.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <returns>The message.</returns>
        internal static string DefaultMessage(XmppErrorCode errorCode, string message = null)
        {
            if (!string.IsNullOrEmpty(message)) return message;

            switch (errorCode)
            {
                case XmppErrorCode.UnrecognizedStream: return Properties.Resources.XmppErrorCode_UnrecognizedStream;
                case XmppErrorCode.BadFormat: return Properties.Resources.XmppErrorCode_BadFormat;
                case XmppErrorCode.BadNamespacePrefix: return Properties.Resources.XmppErrorCode_BadNamespacePrefix;
                case XmppErrorCode.Conflict: return Properties.Resources.XmppErrorCode_Conflict;
                case XmppErrorCode.ConnectionTimeout: return Properties.Resources.XmppErrorCode_ConnectionTimeout;
                case XmppErrorCode.HostGone: return Properties.Resources.XmppErrorCode_HostGone;
                case XmppErrorCode.HostUnknown: return Properties.Resources.XmppErrorCode_HostUnknown;
                case XmppErrorCode.ImproperAddressing: return Properties.Resources.XmppErrorCode_ImproperAddressing;
                case XmppErrorCode.InternalServerError: return Properties.Resources.XmppErrorCode_InternalServerError;
                case XmppErrorCode.InvalidFrom: return Properties.Resources.XmppErrorCode_InvalidFrom;
                case XmppErrorCode.InvalidID: return Properties.Resources.XmppErrorCode_InvalidID;
                case XmppErrorCode.InvalidNamespace: return Properties.Resources.XmppErrorCode_InvalidNamespace;
                case XmppErrorCode.InvalidXml: return Properties.Resources.XmppErrorCode_InvalidXml;
                case XmppErrorCode.NotAuthorized: return Properties.Resources.XmppErrorCode_NotAuthorized;
                case XmppErrorCode.PolicyViolation: return Properties.Resources.XmppErrorCode_PolicyViolation;
                case XmppErrorCode.RemoteConnectionFailed: return Properties.Resources.XmppErrorCode_RemoteConnectionFailed;
                case XmppErrorCode.ResourceConstraint: return Properties.Resources.XmppErrorCode_ResourceConstraint;
                case XmppErrorCode.RestrictedXml: return Properties.Resources.XmppErrorCode_RestrictedXml;
                case XmppErrorCode.SeeOtherHost: return Properties.Resources.XmppErrorCode_SeeOtherHost;
                case XmppErrorCode.SystemShutdown: return Properties.Resources.XmppErrorCode_SystemShutdown;
                case XmppErrorCode.UnsupportedEncoding: return Properties.Resources.XmppErrorCode_UnsupportedEncoding;
                case XmppErrorCode.UnsupportedStanzaType: return Properties.Resources.XmppErrorCode_UnsupportedStanzaType;
                case XmppErrorCode.UnsupportedVersion: return Properties.Resources.XmppErrorCode_UnsupportedVersion;
                case XmppErrorCode.XmlNotWellFormed: return Properties.Resources.XmppErrorCode_XmlNotWellFormed;
                case XmppErrorCode.RequiredFeatureUnknown: return Properties.Resources.XmppErrorCode_RequiredFeatureUnknown;
                case XmppErrorCode.AuthenticationFailed: return Properties.Resources.XmppErrorCode_AuthenticationFailed;
                case XmppErrorCode.AuthenticationAborted: return Properties.Resources.XmppErrorCode_AuthenticationAborted;
                case XmppErrorCode.AccountDisabled: return Properties.Resources.XmppErrorCode_AccountDisabled;
                case XmppErrorCode.CredentialsExpired: return Properties.Resources.XmppErrorCode_CredentialsExpired;
                case XmppErrorCode.EncryptionRequired: return Properties.Resources.XmppErrorCode_EncryptionRequired;
                case XmppErrorCode.InvalidImpersonation: return Properties.Resources.XmppErrorCode_InvalidImpersonation;
                case XmppErrorCode.InvalidMechanism: return Properties.Resources.XmppErrorCode_InvalidMechanism;
                case XmppErrorCode.MalformedRequest: return Properties.Resources.XmppErrorCode_MalformedRequest;
                case XmppErrorCode.MechanismTooWeak: return Properties.Resources.XmppErrorCode_MechanismTooWeak;
                case XmppErrorCode.TemporaryAuthFailure: return Properties.Resources.XmppErrorCode_TemporaryAuthFailure;
                case XmppErrorCode.Stanza: return Properties.Resources.XmppErrorCode_Stanza;
                default: return Properties.Resources.XmppErrorCode_Unknown;
            }
        }
    }
}
