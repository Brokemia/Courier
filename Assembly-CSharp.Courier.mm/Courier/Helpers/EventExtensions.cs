using System;
using System.Reflection;

namespace Mod.Courier.Helpers {
    public static class EventExtensions {
        public static void Raise<TEventArgs>(this object source, string eventName, TEventArgs eventArgs, object[] args = null) where TEventArgs : EventArgs {
            var eventDelegate = (MulticastDelegate)source.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(source);
            if (eventDelegate != null) {
                foreach (var handler in eventDelegate.GetInvocationList()) {
                    handler.Method.Invoke(handler.Target, args);
                }

            }
        }
    }
}
