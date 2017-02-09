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
    public class PropertyChangedTrigger : ITrigger, IDisposable
    {
        private const int PollingTime = 45; // (~15ms .NET response time x 3) should be enough

        private readonly object _objectToObserve;

        private readonly Timer _timer;
        private object _lastPropertyValue;
        private readonly PropertyInfo _propertyInfo;

        public event EventHandler<EventArgs> TriggerOccurred;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNotChangedTrigger"/> class.
        /// </summary>
        /// <param name="objectToObserve">The object to observe.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="DependencyException">If the object or property does not exist.</exception>
        public PropertyChangedTrigger(object objectToObserve, string propertyName)
        {
            _objectToObserve = objectToObserve;

            if (objectToObserve == null)
                throw new DependencyException(
                    string.Format("object for property not changed trigger \"{0}\" must not be null", propertyName));

            _propertyInfo = objectToObserve.GetType().GetProperty(propertyName);

            if (_propertyInfo == null)
                throw new DependencyException(string.Format(
                    "The property name \"{0}\" does not exist in object {1}", propertyName, objectToObserve.GetType().FullName));

            _lastPropertyValue = _propertyInfo.GetValue(objectToObserve);

            _timer = new Timer(OnTimeout, null, PollingTime, Timeout.Infinite);
        }

        private void OnTimeout(object state)
        {
            var currentPropertyValue = _propertyInfo.GetValue(_objectToObserve);

            if (!currentPropertyValue.Equals(_lastPropertyValue))
            {
                var handler = TriggerOccurred;
                if (handler != null)
                {
                    TriggerOccurred(this, new EventArgs());
                }

                _lastPropertyValue = currentPropertyValue;
            }

            _timer.Change(PollingTime, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
