using System;
using System.Reflection;
using Windows.Foundation;

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
    public class WeakEventHandler<TSender, TArgs>
        where TSender : class
    {
        // WeakReference to the event sink, which may be outlived by the event source
        private readonly WeakReference targetReference;

        // The event handler/method to invoke
        private readonly MethodInfo wrappedHandler;

        /// <summary>
        /// Creates a new weak EventHandler from the "real" EventHandler provided.
        /// </summary>
        /// <param name="callback">The EventHandler to wrap.</param>
        public WeakEventHandler(TypedEventHandler<TSender, TArgs> callback)
            : this((Delegate)callback)
        { }

        /// <summary>
        /// Protected constructor for initializing fields.
        /// </summary>
        /// <param name="callback">The EventHandler to wrap.</param>
        protected WeakEventHandler(Delegate callback)
        {
            this.wrappedHandler = callback.GetMethodInfo();
            this.targetReference = new WeakReference(callback.Target, true);
        }

        /// <summary>
        /// Serves as the EventHandler that is actually attached to the desired event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Handler(TSender sender, TArgs e)
        {
            object target = this.targetReference.Target;
            if (target != null)
            {
                var callback = (Action<TSender, TArgs>)
                    this.wrappedHandler.CreateDelegate(typeof(Action<TSender, TArgs>), target);
                if (callback != null)
                {
                    callback(sender, e);
                }
            }
        }
    }

    public sealed class WeakEventHandler<TEventArgs> : WeakEventHandler<object, TEventArgs>
        where TEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new weak EventHandler from the "real" EventHandler provided.
        /// </summary>
        /// <param name="callback">The EventHandler to wrap.</param>
        public WeakEventHandler(EventHandler<TEventArgs> callback)
            : base(callback)
        { }
    }
}
