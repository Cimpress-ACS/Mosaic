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
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    public class FluentTagConditionInterface
    {
        private readonly LooseTagStorage _looseTagStorage;
        private Tag _tag;
        private FluentTagThenInterface _fluentTagThenInterface;
        private Func<object, bool> _condition;

        public FluentTagConditionInterface(LooseTagStorage looseTagStorage, Tag tag)
        {
            _looseTagStorage = looseTagStorage;
            _tag = tag;
        }

        // it is possible to set the tag later (for lazy evaluation, needed for TagController).
        internal void SetTag(Tag tag)
        {
            _tag = tag;
            _fluentTagThenInterface.SetTag(tag);
        }

        internal void CheckCondition(object newValue)
        {
            if (newValue == null)
                return;

            if (_condition(newValue))
                _fluentTagThenInterface.PerformAction();
        }

        /// <summary>
        /// Adds an equals condition to the behavior statement.
        /// </summary>
        /// <param name="valueIs">The value to compare for.</param>
        public FluentTagThenInterface ValueEquals(object valueIs)
        {
            _condition = newValue => newValue.Equals(valueIs);
            _fluentTagThenInterface = new FluentTagThenInterface(_looseTagStorage, _tag);
            return _fluentTagThenInterface;
        }

        /// <summary>
        /// Adds an greater-than condition to the behavior statement.
        /// </summary>
        /// <param name="greaterThanValue">Check if tag value is greater than this specified value.</param>
        public FluentTagThenInterface ValueGreaterThan(object greaterThanValue)
        {
            _condition = newValue =>
                         {
                             var v1 = (IComparable) newValue;
                             var v2 = (IComparable) greaterThanValue;
                             return v1.CompareTo(v2) > 0;
                         };
            _fluentTagThenInterface = new FluentTagThenInterface(_looseTagStorage, _tag);
            return _fluentTagThenInterface;
        }

        /// <summary>
        /// Adds an greater-than condition to the behavior statement.
        /// </summary>
        /// <param name="lessThanValue">Check if tag value is less than this specified value.</param>
        public FluentTagThenInterface ValueLessThan(object lessThanValue)
        {
            _condition = newValue =>
            {
                var v1 = (IComparable)newValue;
                var v2 = (IComparable)lessThanValue;
                return v1.CompareTo(v2) < 0;
            };
            _fluentTagThenInterface = new FluentTagThenInterface(_looseTagStorage, _tag);
            return _fluentTagThenInterface;
        }

        internal void Takt()
        {
            _fluentTagThenInterface.Takt();
        }
    }
}
