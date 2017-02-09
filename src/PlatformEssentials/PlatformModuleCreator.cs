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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Credentials;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Events;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;
using VP.FF.PT.Common.PlatformEssentials.ModuleWiringConfigSection;

namespace VP.FF.PT.Common.PlatformEssentials
{
    [Export(typeof(IPlatformModuleCreator))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlatformModuleCreator : IPlatformModuleCreator
    {
        private const int UndefinedStreamType = 0;
        private readonly IPlatformModuleRepository _moduleRepository;
        private readonly ILogger _logger;
        private readonly IPlatformDependencyManager _dependencyManager;
        private readonly IProvideConfiguration _provideConfiguration;
        private bool _allModulesCreated;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<CompositionContainer> _container = null;

        [Import]
        internal protected IEventAggregator EventAggregator { get; internal set; }

        private readonly IList<IPlatformModuleFactory> _usedFactories = new List<IPlatformModuleFactory>();
        private ModuleWiringConfigSection.ModuleWiringConfigSection _configSection;
        private int _streamType;

        /// <summary>
        /// Initializes a new <see cref="PlatformModuleCreator"/> instance.
        /// </summary>
        /// <param name="moduleRepository">An instance of the global module repository. The initializer is responsible to populate this container.</param>
        /// <param name="provideConfiguration"></param>
        /// <param name="logger">The logger.</param>
        /// <param name="dependencyManager">The dependency manager.</param>
        [ImportingConstructor]
        public PlatformModuleCreator(
            IPlatformModuleRepository moduleRepository, 
            IProvideConfiguration provideConfiguration, 
            ILogger logger,
            IPlatformDependencyManager dependencyManager)
        {
            _moduleRepository = moduleRepository;
            _provideConfiguration = provideConfiguration;
            _logger = logger;
            _dependencyManager = dependencyManager;
            _logger.Init(GetType());
        }

        private void Construct(int streamType)
        {
            foreach (var module in _moduleRepository.Modules)
            {
                if (module.StreamType != streamType)
                    continue;

                try
                {
                    _logger.Info("Constructing module " + module.Name);
                    module.Construct();
                    _logger.Info("Constructed module " + module.Name);

                }
                catch (Exception e)
                {
                    module.Disable();
                    _logger.Error("Can't construct module " + module.Name + "!", e);
                }
            }
        }

        public bool AllModulesCreated
        {
            get
            {
                if (_allModulesCreated)
                    return true;

                foreach (var module in _moduleRepository.Modules)
                {
                    if (!string.IsNullOrEmpty(module.Name) && module.State == PlatformModuleState.Undefined)
                        return false;
                }

                _allModulesCreated = true;
                return true;
            }
        }

        public void ConstructModules(int streamType)
        {
            _configSection = _provideConfiguration.GetConfiguration<ModuleWiringConfigSection.ModuleWiringConfigSection>("moduleWiring" + streamType);
            _streamType = streamType;

            CreateModules(_configSection, streamType);

            foreach (var factory in _usedFactories)
            {
                factory.TriggerContainerRecomposition();
            }

            CreateGraphAndWireUp();

            Construct(streamType);

            CreateDependencies();

            EventAggregator.Publish(new AllModulesCreatedEvent());

        }

        private void CreateGraphAndWireUp()
        {
            if (_streamType == UndefinedStreamType)
                return;

            try
            {
                Graph = CreateGraph(_configSection);
            }
            catch (ConfigurationErrorsException e)
            {
                _logger.Error("Errors when reading the configuration section \"" + _configSection + "\"", e);
                throw new Exception("Cannot wire the modules together", e);
            }
        }

        public ModuleGraph Graph { get; private set; }

        private ModuleGraph CreateGraph(ModuleWiringConfigSection.ModuleWiringConfigSection configSection)
        {
            var graph = new ModuleGraph();

            // create graph vertices
            foreach (ModuleConfig moduleConfig in configSection.Modules)
            {
                var module = GetModuleByName(moduleConfig.Name);
                if (module == null)
                    continue;

                // create QuickGraph datastructure
                graph.AddVertex(module);
            }

            // create graph edges
            foreach (ModuleConfig moduleConfig in configSection.Modules)
            {
                var module = GetModuleByName(moduleConfig.Name);
                if (module == null)
                    continue;

                foreach (ModuleReferenceConfig targetModuleConfig in moduleConfig.NextModules)
                {
                    var targetModule = GetModuleByName(targetModuleConfig.Name);

                    if (targetModule == null)
                        continue;

                    var edge = new ModuleGraphEdge(
                                                module.Name + "->" + targetModule.Name + ":" + targetModuleConfig.Port + targetModuleConfig.TargetPort,
                                                module,
                                                targetModule,
                                                targetModuleConfig.Port,
                                                targetModuleConfig.TargetPort);
                    graph.AddEdge(edge);
                }
            }

            // enable forcing for one-way edges by default
            var modulesWithSingleEdge = from e in graph.Edges
                                        group e by e.Source into g
                                        where g.Count() == 1
                                        select g.Key;

            foreach (var module in modulesWithSingleEdge)
            {
                var oneWayEdge = (from e in graph.Edges
                                  where e.Source == module
                                  select e).Single();

                if (!IsCyclicEdge(graph, oneWayEdge))
                    oneWayEdge.IsForcingEnabled = true;
            }

            return graph;
        }

        private void CreateDependencies()
        {
            foreach (ModuleConfig moduleConfig in _configSection.Modules)
            {
                var module = GetModuleByName(moduleConfig.Name);
                if (module == null)
                    continue;

                foreach (ModuleDependencyConfig dependencyConfig in moduleConfig.Dependencies)
                {
                    var logger = _container.First().GetExportedValue<ILogger>();
                    logger.Init(module.GetType() + "-Dependency");

                    var dependency = new ModuleDependency(logger);

                    // given
                    foreach (ConditionConfigBase conditionConfig in dependencyConfig.Conditions)
                    {
                        var propertyEqualConditionConfig = conditionConfig as PropertyEqualsConditionConfig;
                        if (propertyEqualConditionConfig != null)
                        {
                            dependency.AddCondition(new PropertyEqualityCondition(
                                                            module, 
                                                            propertyEqualConditionConfig.PropertyName, 
                                                            propertyEqualConditionConfig.EqualsValue));
                            continue;
                        }
                    }

                    // when
                    foreach (TriggerConfigBase triggerConfig in dependencyConfig.Triggers)
                    {
                        var eventTriggerConfig = triggerConfig as EventTriggerConfig;
                        if (eventTriggerConfig != null)
                        {
                            dependency.AddTrigger(new EventTrigger(GetModuleByName(eventTriggerConfig.Path), eventTriggerConfig.EventName));
                            continue;
                        }

                        var eventNotRaisedTriggerConfig = triggerConfig as EventNotRaisedTriggerConfig;
                        if (eventNotRaisedTriggerConfig != null)
                        {
                            dependency.AddTrigger(new EventNotRaisedTrigger(
                                                            GetModuleByName(eventNotRaisedTriggerConfig.Path), 
                                                            eventNotRaisedTriggerConfig.EventName,
                                                            TimeSpan.FromMilliseconds(eventNotRaisedTriggerConfig.Timeout)));
                            continue;
                        }

                        var propertyChangedTriggerConfig = triggerConfig as PropertyChangedTriggerConfig;
                        if (propertyChangedTriggerConfig != null)
                        {
                            dependency.AddTrigger(new PropertyChangedTrigger(
                                                        GetModuleByName(propertyChangedTriggerConfig.Path),
                                                        propertyChangedTriggerConfig.PropertyName));
                            continue;
                        }

                        var propertyNotChangedTriggerConfig = triggerConfig as PropertyNotChangedTriggerConfig;
                        if (propertyNotChangedTriggerConfig != null)
                        {
                            dependency.AddTrigger(new PropertyNotChangedTrigger(
                                                        GetModuleByName(propertyNotChangedTriggerConfig.Path), 
                                                        propertyNotChangedTriggerConfig.PropertyName, 
                                                        TimeSpan.FromMilliseconds(propertyNotChangedTriggerConfig.Timeout)));
                            continue;
                        }

                        var periodicTimeTriggerConfig = triggerConfig as PeriodicTimeTriggerConfig;
                        if (periodicTimeTriggerConfig != null)
                        {
                            dependency.AddTrigger(new PeriodicTimeTrigger(TimeSpan.FromMilliseconds(periodicTimeTriggerConfig.Timeout)));
                            continue;
                        }
                    }

                    // then
                    foreach (var actionConfig in dependencyConfig.Actions)
                    {
                        var invokeMethodActionConfig = actionConfig as InvokeMethodActionConfig;
                        if (invokeMethodActionConfig != null)
                        {
                            dependency.AddAction(new InvokeMethodAction(module, invokeMethodActionConfig.MethodName));
                            continue;
                        }
                    }

                    _dependencyManager.Add(dependency);
                    _logger.Debug("added module dependency to " + module);
                }
            }
        }

        private IPlatformModule GetModuleByName(string name)
        {
            var res = from m in _moduleRepository.Modules
                      where !string.IsNullOrEmpty(m.Name) && m.Name.Equals(name)
                      select m;

            if (!res.Any())
            {
                return null;
            }

            return res.First();
        }

        private IPlatformModule CreateModuleByType(int moduleTypeId)
        {
            var container = _container.First();

            try
            {
                var factory = container.GetExportedValue<IPlatformModuleFactory>(moduleTypeId.ToString());

                if (!_usedFactories.Contains(factory))
                    _usedFactories.Add(factory);

                var module = factory.CreateModule();

                _moduleRepository.AddNewModule(module);

                return module;
            }
            catch (Exception e)
            {
                _logger.Error("Can't create module type " + moduleTypeId + " because no matching factory was found in MEF container", e);
                return null;
            }
        }

        private bool IsCyclicEdge(ModuleGraph graph, ModuleGraphEdge edge)
        {
            var cyclicEdge = (from e in graph.Edges
                              where e.Source == edge.Target &&
                                    e.Target == edge.Source
                              select e).FirstOrDefault();

            if (cyclicEdge != null)
                return true;
            return false;
        }

        private void CreateModules(ModuleWiringConfigSection.ModuleWiringConfigSection configSection, int streamType)
        {
            foreach (ModuleConfig moduleConfig in configSection.Modules)
            {
                var module = GetModuleByName(moduleConfig.Name);

                // try to create new module if not available (at least one instance is exported by MEF)
                if (module == null || module.AdsPort != 0)
                {
                    module = CreateModuleByType(moduleConfig.ModuleTypeId);

                    if (module == null)
                        continue;

                    _logger.Debug("Created module " + moduleConfig.Name + " (ID=" + moduleConfig.ModuleTypeId + ")");
                }

                // set parameters from App.config to module
                module.StreamType = streamType;
                module.ModuleTypeId = moduleConfig.ModuleTypeId;
                module.Name = moduleConfig.Name;

                int port = 0;
                Int32.TryParse(moduleConfig.Config.PlcPort, out port);
                module.AdsPort = port;

                module.PathRootController = moduleConfig.Config.PlcRootController;
                module.PlcAddress = moduleConfig.Config.PlcAddress;
                module.EnableStateLogging = bool.Parse(moduleConfig.Config.EnableStateLogging);
                module.EnableSubStateLogging = bool.Parse(moduleConfig.Config.EnableSubStateLogging);
                module.ModuleNbr = int.Parse(moduleConfig.Config.ModuleNbr);
                module.PlannedThroughput = int.Parse(moduleConfig.Config.PlannedThroughputPerHour);
            }
        }

    }
}
