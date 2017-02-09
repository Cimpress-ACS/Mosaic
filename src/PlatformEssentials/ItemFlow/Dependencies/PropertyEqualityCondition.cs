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


using System.Reflection;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies
{
    public class PropertyEqualityCondition : ICondition
    {
        private readonly object _checkObjectInstance;
        private readonly object _expectedObjectValue;
        private readonly string _expectedStringValue;
        private PropertyInfo _propertyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEqualityCondition"/> class.
        /// </summary>
        /// <param name="checkObjectInstance">The check object instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="expectedObjectValue">The expected value.</param>
        /// <exception cref="DependencyException">If the object or property does not exist.</exception>
        public PropertyEqualityCondition(object checkObjectInstance, string propertyName, object expectedObjectValue)
        {
            _checkObjectInstance = checkObjectInstance;
            _expectedObjectValue = expectedObjectValue;
            Construct(propertyName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEqualityCondition"/> class with a string compare.
        /// </summary>
        /// <param name="checkObjectInstance">The check object instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="expectedValueAsString">The expected value represented as a string (just call ToString() for the object).</param>
        /// <exception cref="DependencyException">If the object or property does not exist.</exception>
        public PropertyEqualityCondition(object checkObjectInstance, string propertyName, string expectedValueAsString)
        {
            _checkObjectInstance = checkObjectInstance;
            _expectedStringValue = expectedValueAsString;
            Construct(propertyName);
        }

        private void Construct(string propertyName)
        {
            if (_checkObjectInstance == null)
                throw new DependencyException(
                    string.Format("the object for the property condition check \"{0}\" must not be null", propertyName));

            _propertyInfo = _checkObjectInstance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (_propertyInfo == null)
                throw new DependencyException(string.Format(
                    "the property \"{0}\" does not exist in object {1}", propertyName, _checkObjectInstance.GetType().FullName));
        }

        public string PathToObject { get; set; }

        public bool IsTrue()
        {
            if (_expectedStringValue != null)
                return _propertyInfo.GetValue(_checkObjectInstance).ToString().Equals(_expectedStringValue);

            return _propertyInfo.GetValue(_checkObjectInstance).Equals(_expectedObjectValue);
        }
    }
}
