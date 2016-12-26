using System;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// An attribute that indicates that a method should be subscribed as an event handler
    /// for a given event name.
    /// Using: [AutoWire(nameof(Target.EventName))]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AutoWireAttribute : Attribute
    {
        private readonly string eventName;

        /// <summary>
        /// Initializes the attribute with the event to register for.
        /// </summary>
        /// <param name="eventName"></param>
        public AutoWireAttribute(string eventName)
        {
            this.eventName = eventName;
        }

        /// <summary>
        /// The event that should be registered for.
        /// </summary>
        public string EventName
        {
            get { return this.eventName; }
        }
    }
}
