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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    [Export(typeof(ISimulatedPlcBehaviorManager))]
    [Export(typeof(ISimulatedBehaviorManagerInternal))]
    public class SimulatedPlcBehaviorManager : ISimulatedPlcBehaviorManager, ISimulatedBehaviorManagerInternal
    {
        private readonly LooseTagStorage _looseTagStorage;
        private readonly ILogger _logger;
        private readonly ICollection<ITagListener> _tagListeners = new Collection<ITagListener>();
        private readonly ICollection<ITagController> _tagControllers = new Collection<ITagController>();
        private readonly ICollection<ITagImporter> _tagImporters = new Collection<ITagImporter>(); 
        private readonly ICollection<IAlarmsImporter> _alarmsImporters = new Collection<IAlarmsImporter>();
        private readonly ICollection<IControllerTreeImporter> _controllerTreeImporters = new Collection<IControllerTreeImporter>();

        private readonly List<PeriodicAction> _periodicActions = new List<PeriodicAction>();
        private readonly IList<FluentTagConditionInterface> _fluentItems = new List<FluentTagConditionInterface>();

        private class PeriodicAction
        {
            public Func<bool> Condition { get; set; }
            public Action Action { get; set; }
            public int ExecuteAfterTakts { get; set; }
            public int CurrentTaktCount { get; set; }
        }

        public event EventHandler Initialized;

        [ImportingConstructor]
        public SimulatedPlcBehaviorManager(LooseTagStorage looseTagStorage, ILogger logger)
        {
            _looseTagStorage = looseTagStorage;
            _logger = logger;
        }

        [ImportMany(typeof(ISimulatedPlcBehavior))] 
        private IEnumerable<Lazy<ISimulatedPlcBehavior>> _behaviors = null;

        public FluentTagConditionInterface WhenTag(Tag tag)
        {
            var fluentItem = new FluentTagConditionInterface(_looseTagStorage, tag);
            _fluentItems.Add(fluentItem);

            if (tag != null)
                tag.ValueChanged += (sender, args)
                    => fluentItem.CheckCondition(sender.Value);

            return fluentItem;
        }

        private class LazyTagChangedEvaluationStruct
        {
            public string TagName;
            public FluentTagConditionInterface FluentTagCondition;
        }

        private readonly IList<LazyTagChangedEvaluationStruct> _lazyTagChangedEvalutions = new List<LazyTagChangedEvaluationStruct>(); 

        public FluentTagConditionInterface WhenTag(string tagFullName, int port = 0)
        {
            var tag = SearchTag(tagFullName, port);

            var fluentInterface = WhenTag(tag);

            if (tag == null)
            {
                // lazy evaluation
                _lazyTagChangedEvalutions.Add(new LazyTagChangedEvaluationStruct
                                                {
                                                    TagName = tagFullName,
                                                    FluentTagCondition = fluentInterface
                                                });
            }

            return fluentInterface;
        }

        private void OnTagControllerTagWritten(object sender, Tag e)
        {
            // evaluate if there is a behavior subscribed to this Tag
            foreach (var lazyTagEvaluation in _lazyTagChangedEvalutions)
            {
                if ((e.Scope + "." + e.Name) == lazyTagEvaluation.TagName)
                {
                    lazyTagEvaluation.FluentTagCondition.SetTag(e);
                    lazyTagEvaluation.FluentTagCondition.CheckCondition(e.Value);
                }
            }

            // connect and inject value back to TagListener if there is a Tag with similar scope.name
            // this is needed because there is no connection of TagController tags and TagListener tags (normally its "connected" on real PLC)
            foreach (var tagListener in _tagListeners)
            {
                var otherTag = tagListener.GetTags().FirstOrDefault(t => t.Scope == e.Scope && t.Name == e.Name && t.AdsPort == e.AdsPort);

                if (otherTag != null)
                    otherTag.Value = e.Value;
            }
        }

        public void AddPeriodicAction(int executeAfterTakts, Action action)
        {
            _periodicActions.Add(new PeriodicAction
            {
                Action = action, 
                ExecuteAfterTakts = executeAfterTakts
            });
        }

        public void AddPeriodicAction(int executeAfterTakts, Func<bool> condition, Action action)
        {
            _periodicActions.Add(new PeriodicAction
            {
                Condition = condition,
                Action = action,
                ExecuteAfterTakts = executeAfterTakts
            });
        }

        public void Takt()
        {
            _fluentItems.ForEach(i => i.Takt());

            foreach (var periodicAction in _periodicActions)
            {
                periodicAction.CurrentTaktCount++;

                try
                {
                    if (periodicAction.CurrentTaktCount >= periodicAction.ExecuteAfterTakts)
                    {
                        periodicAction.CurrentTaktCount = 0;

                        var condition = periodicAction.Condition ?? (() => true);
                        if (condition())
                        {
                            periodicAction.Action();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Simulation crash in a periodic task", e);
                }
            }
        }

        public void Initialize()
        {
            foreach (var simulatedBehavior in _behaviors)
            {
                simulatedBehavior.Value.Initialize(this);
            }

            if (Initialized != null)
                Initialized(this, new EventArgs());
        }

        public void AddTagListener(ITagListener tagListener)
        {
            if (tagListener != null)
                _tagListeners.Add(tagListener);
        }

        public void AddTagController(ITagController tagController)
        {
            if (tagController != null)
            {
                ((SimulatedTagController)tagController).TagWrittenEvent += OnTagControllerTagWritten;
                _tagControllers.Add(tagController);
            }
        }

        public void AddTagImporter(ITagImporter tagImporter)
        {
            if (tagImporter != null)
                _tagImporters.Add(tagImporter);
        }

        public void AddAlarmsImporter(IAlarmsImporter alarmsImporter)
        {
            if (alarmsImporter != null)
                _alarmsImporters.Add(alarmsImporter);
        }

        public void AddControllerTreeImporter(IControllerTreeImporter controllerTreeImporter)
        {
            if (controllerTreeImporter != null)
                _controllerTreeImporters.Add(controllerTreeImporter);
        }

        public Tag SearchTag(string tagFullName, int port = 0)
        {
            return _tagListeners.SelectMany(l => l.GetTags())
                .FirstOrDefault(t => port == 0 ? 
                                            tagFullName.Equals(t.Scope + "." + t.Name, StringComparison.InvariantCultureIgnoreCase) :
                                            tagFullName.Equals(t.Scope + "." + t.Name, StringComparison.InvariantCultureIgnoreCase) && t.AdsPort == port);
        }

        public Tag SearchOrCreateLooseTag(string tagFullName, int port = 0)
        {
            return SearchTag(tagFullName, port) ?? _looseTagStorage.GetOrCreateTag(tagFullName, port);
        }
    }
}
