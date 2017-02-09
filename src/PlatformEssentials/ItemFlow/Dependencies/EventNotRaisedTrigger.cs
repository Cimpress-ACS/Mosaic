/* Copyright 2017 Cimpress

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. */


using System;
using System.Reflection;
using System.Threading;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies
{
    public class EventNotRaisedTrigger : ITrigger, IDisposable
    {
        private readonly TimeSpan _timeout;
        public event EventHandler<EventArgs> TriggerOccurred;

        private readonly Timer _timer;
        private EventArgs _eventArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotChangedTrigger"/> class.
        /// </summary>
        /// <param name="objectToObserve">The object to observe.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="timeout">The timeout.</param>
        /// <exception cref="DependencyException">If the object or event does not exist.</exception>
        public EventNotRaisedTrigger(object objectToObserve, string eventName, TimeSpan timeout)
        {
            _timeout = timeout;

            if (objectToObserve == null)
                throw new DependencyException(
                    string.Format("object for event not raised trigger \"{0}\" must not be null", eventName));

            EventInfo eventInfo = objectToObserve.GetType().GetEvent(eventName);

            if (eventInfo == null)
                throw new DependencyException(string.Format(
                    "The event name \"{0}\" does not exist in object {1}", eventName, objectToObserve.GetType().FullName));

            // signature of event handler must macht the event type. EventArgs is required as a base type
            MethodInfo eventHandlerMethod = GetType().GetMethod("OnEvent", BindingFlags.NonPublic | BindingFlags.Instance);

            Type eventDelegateType = eventInfo.EventHandlerType;

            Delegate eventDelegate = Delegate.CreateDelegate(eventDelegateType, this, eventHandlerMethod);

            // += event handler
            MethodInfo addHandler = eventInfo.GetAddMethod();
            Object[] addHandlerArgs = { eventDelegate };
            addHandler.Invoke(objectToObserve, addHandlerArgs);

            _timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void OnTimeout(object state)
        {
            var handler = TriggerOccurred;
            if (handler != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                TriggerOccurred(this, _eventArgs);
            }
        }

        // ReSharper disable once UnusedMember.Local because this method is used by Reflection
        private void OnEvent(object sender, EventArgs args)
        {
            _eventArgs = args;
            _timer.Change((int) _timeout.TotalMilliseconds, 0);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
