using System;
using System.Reflection;

namespace SariphLib.Eventing
{
    /// <summary>
    /// Represents a wrapper around an EventHandler, which is designed to only add a weak 
    /// reference to the event sink.
    /// </summary>
    /// <remarks>
    /// Adapted from work by Paul Stovell (Thanks!)
    /// http://paulstovell.com/blog/weakevents
    /// 
    /// Note that his implementation is imperfect - the WeakEventHandler Handle is "sacrificed", in
    /// that it never unsubscribes itself from the target Event. This is considered a reasonable
    /// trade-off for now as memory usage should still drastically improve.
    /// </remarks>
    /// <typeparam name="TEventArgs">The type of EventArgs used by the event.</typeparam>
    public sealed class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        // WeakReference to the event sink, which may be outlived by the event source
        private readonly WeakReference targetReference;

        // The event handler/method to invoke
        private readonly MethodInfo wrappedHandler;

        /// <summary>
        /// Creates a new weak EventHandler from the "real" EventHandler provided.
        /// </summary>
        /// <param name="callback">The EventHandler to wrap.</param>
        public WeakEventHandler(EventHandler<TEventArgs> callback)
        {
            this.wrappedHandler = callback.GetMethodInfo();
            this.targetReference = new WeakReference(callback.Target, true);
        }

        /// <summary>
        /// Serves as the EventHandler that is actually attached to the desired event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Handler(object sender, TEventArgs e)
        {
            object target = this.targetReference.Target;
            if (target != null)
            {
                var callback = (Action<object, TEventArgs>)
                    this.wrappedHandler.CreateDelegate(typeof(Action<object, TEventArgs>), target);
                if (callback != null)
                {
                    callback(sender, e);
                }
            }
        }
    }
}
