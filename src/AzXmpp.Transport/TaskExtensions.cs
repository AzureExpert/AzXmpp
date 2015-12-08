using System;
using System.Threading.Tasks;

namespace AzXmpp.Transport
{
    /// <summary>
    /// Represents extension methods for <see cref="Task"/>.
    /// </summary>
    static class TaskExtensions
    {
        /// <summary>
        /// Converts the specified <see cref="Task{TResult}"/> to the asynchronous programming model.
        /// </summary>
        /// <typeparam name="TResult">The result type of the task.</typeparam>
        /// <param name="task">The task to convert.</param>
        /// <param name="callback">The APM callback.</param>
        /// <param name="state">The APM state.</param>
        /// <returns>The APM <see cref="IAsyncResult"/>.</returns>
        public static IAsyncResult AsApm<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<TResult>(state);
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) tcs.TrySetCanceled();
                else tcs.TrySetResult(t.Result);

                if (callback != null) callback(tcs.Task);
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>
        /// Converts the specified <see cref="Task"/> to the asynchronous programming model.
        /// </summary>
        /// <param name="task">The task to convert.</param>
        /// <param name="callback">The APM callback.</param>
        /// <param name="state">The APM state.</param>
        /// <returns>The APM <see cref="IAsyncResult"/>.</returns>
        public static IAsyncResult AsApm(this Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<int>(state);
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) tcs.TrySetCanceled();
                else tcs.TrySetResult(0);

                if (callback != null) callback(tcs.Task);
            }, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}
