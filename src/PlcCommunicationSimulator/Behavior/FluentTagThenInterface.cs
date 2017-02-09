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
using System.Collections.Concurrent;
using System.Collections.Generic;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    public class FluentTagThenInterface
    {
        private readonly LooseTagStorage _looseTagStorage;
        private Tag _tag;

        public FluentTagThenInterface(LooseTagStorage looseTagStorage, Tag tag)
        {
            _looseTagStorage = looseTagStorage;
            _tag = tag;
        }

        // it is possible to set the tag later (for lazy evaluation, needed for TagController).
        internal void SetTag(Tag tag)
        {
            _tag = tag;
        }

        private class FluentThenItem
        {
            public Action Action;
            public int ExecuteAfterTakts;
            public int CurrentTaktCount;
            public Func<bool> ContinueThenExecution = () => true;
        }

        private readonly IList<FluentThenItem> _doSomethingItems = new List<FluentThenItem>();
        private readonly ConcurrentQueue<FluentThenItem> _executeItemsQueue = new ConcurrentQueue<FluentThenItem>();

        internal void PerformAction()
        {
            _executeItemsQueue.Clear();

            foreach (var item in _doSomethingItems)
            {
                item.CurrentTaktCount = 0;
                _executeItemsQueue.Enqueue(item);
            }
        }

        internal void Takt()
        {
            FluentThenItem currentItem;
            if (!_executeItemsQueue.TryPeek(out currentItem))
            {
                return;
            }

            if (!currentItem.ContinueThenExecution())
            {
                _executeItemsQueue.Clear();
                return;
            }

            currentItem.CurrentTaktCount++;

            if (currentItem.CurrentTaktCount >= currentItem.ExecuteAfterTakts)
            {
                currentItem.Action();

                FluentThenItem tmp;
                while(!_executeItemsQueue.TryDequeue(out tmp))
                { }
            }
        }

        public FluentTagThenInterface When(Func<bool> condition)
        {
            var thenItem = new FluentThenItem();
            thenItem.Action = () => { };
            thenItem.ContinueThenExecution = condition;

            _doSomethingItems.Add(thenItem);

            return this;
        }

        /// <summary>
        /// Defines an action to perform if the previous condition is true.
        /// </summary>
        /// <param name="doSomethingAction">The do something action.</param>
        /// <returns>Returns fluent interface to add another Then statements in a row (optional).</returns>
        public FluentTagThenInterface Then(Action doSomethingAction)
        {
            _doSomethingItems.Add(new FluentThenItem
                                  {
                                      Action = doSomethingAction
                                  });
            return this;
        }

        /// <summary>
        /// Defines an action to perform if the previous condition is true with a delay.
        /// </summary>
        /// <param name="executeAfterTakts">The action will be executed after every N-th takt.</param>
        /// <param name="doSomethingAction">The do something action.</param>
        /// <returns>Returns fluent interface to add another Then statements in a row (optional).</returns>
        public FluentTagThenInterface ThenWithDelay(int executeAfterTakts, Action doSomethingAction)
        {

            _doSomethingItems.Add(new FluentThenItem
                                  {
                                      Action = doSomethingAction,
                                      ExecuteAfterTakts = executeAfterTakts
                                  });
            return this;
        }

        /// <summary>
        /// Writes a value to the tag if the condition is true.
        /// </summary>
        /// <param name="writeValue">The value to write.</param>
        /// <returns>Returns fluent interface to add another Then statements in a row (optional).</returns>
        public FluentTagThenInterface ThenWriteValue(object writeValue)
        {
            _doSomethingItems.Add(new FluentThenItem
            {
                Action = () => { _tag.Value = writeValue; }
            });
            return this;
        }

        /// <summary>
        /// Writes a value to the tag if the condition is true with a delay.
        /// </summary>
        /// <param name="executeAfterTakts">The action will be executed after ever N-th takt.</param>
        /// <param name="writeValue">The value to write.</param>
        /// <returns>Returns fluent interface to add another Then statements in a row (optional).</returns>
        public FluentTagThenInterface ThenWriteValueWithDelay(int executeAfterTakts, object writeValue)
        {
            _doSomethingItems.Add(new FluentThenItem
                                  {
                                      Action = () => { _tag.Value = writeValue; },
                                      ExecuteAfterTakts = executeAfterTakts
                                  });
            return this;
        }

        /// <summary>
        /// Cancels the whole then-execution-chain when the Tag value changes.
        /// It will ignore the Tag change withing a then-action-statement.
        /// Can be combined with other Tags.
        /// </summary>
        /// <param name="tag">The tag to listen for value changes.</param>
        /// <example>
        /// plcBehaviorManager
        ///     .WhenTag(commandChannelTag)
        ///     .ValueEquals("RUN")
        ///     .Then(() => stateTag.Value = "RUN")
        ///     .CancelOnTagChange(stateTag);
        /// </example>
        /// <returns>Returns fluent interface to add another Then or cancel statements in a row (optional).</returns>
        public FluentTagThenInterface CancelOnTagChange(Tag tag)
        {
            tag.ValueChanged += delegate
            {
                // clear queue
                FluentThenItem ignored;
                while (_executeItemsQueue.TryDequeue(out ignored))
                { }
            };

            return this;
        }

        /// <summary>
        /// Cancels the whole then-execution-chain when the Tag value changes.
        /// It will ignore the Tag change withing a then-action-statement.
        /// Can be combined with other Tags.
        /// </summary>
        /// <param name="commandChannelTag">The tag to listen for value changes.</param>
        /// <param name="port">The port (optional).</param>
        /// <returns>
        /// Returns fluent interface to add another Then or cancel statements in a row (optional).
        /// </returns>
        /// <example>
        /// plcBehaviorManager
        /// .WhenTag(commandChannelTag)
        /// .ValueEquals("RUN")
        /// .Then(() =&gt; stateTag.Value = "RUN")
        /// .CancelOnTagChange("MAIN.fb_1.cmdTag");
        /// </example>
        public FluentTagThenInterface CancelOnTagChange(string commandChannelTag, int port = 0)
        {
            var tag = _looseTagStorage.GetOrCreateTag(commandChannelTag, port);
            return CancelOnTagChange(tag);
        }
    }
}
