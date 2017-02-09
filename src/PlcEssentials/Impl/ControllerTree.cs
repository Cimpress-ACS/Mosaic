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
using System.Linq;
using System.Text;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    /// <summary>
    /// The <see cref="ControllerTree"/> provides interaction
    /// possibilities with a whole controller tree (instead of a single controller).
    /// </summary>
    public class ControllerTree : IControllerTree
    {
        private const uint INVALID_ALARMID = 0;
        private static readonly object _lockObject = new object();

        private Controller _rootController;

        public bool IsInitialized { get; private set; }

        public ControllerTree()
        {
            AlarmsRemoved += removedAlarms => { };
            AlarmsAdded += addedAlarms => { };
            AlarmsChanged += changedAlarms => { };
            OverallInformationChanged += () => { };
        }
        
        /// <summary>
        /// Initializes the new <see cref="ControllerTree"/> instance.
        /// </summary>
        /// <param name="rootController">The root controller of the tree.</param>
        public void Initialize(Controller rootController)
        {
            _rootController = rootController;
            _rootController.VisitAllNodes(c => c.ChildsCollection, c => c.CommonInformationChanged += () => OverallInformationChanged());
            IsInitialized = true;
        }

        /// <summary>
        /// The event <see cref="AlarmsRemoved"/> is raised if old alarms were removed
        /// </summary>
        public event Action<IEnumerable<Alarm>> AlarmsRemoved;

        /// <summary>
        /// The event <see cref="AlarmsAdded"/> is raised if new alarms were added
        /// </summary>
        public event Action<IEnumerable<Alarm>> AlarmsAdded;

        /// <summary>
        /// The event <see cref="AlarmsChanged"/> is raised if alarms were replaced
        /// </summary>
        public event Action<IEnumerable<Alarm>> AlarmsChanged;

        /// <summary>
        /// The event <see cref="OverallInformationChanged"/> is raised when ever a controller in this tree
        /// changed its overall information.
        /// </summary>
        public event Action OverallInformationChanged;

        /// <summary>
        /// Gets the root controller of the tree.
        /// </summary>
        public Controller RootController
        {
            get { return _rootController; }
        }

        /// <summary>
        /// Gets the root controller of the tree.
        /// </summary>
        IController IControllerTree.RootController
        {
            get { return RootController; }
        }

        /// <summary>
        /// Gets the controller with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the desired controller.</param>
        /// <exception cref="InvalidOperationException">
        /// Throws an invalid operation exception if the desired controller does no exist.
        /// </exception>
        /// <returns>An <see cref="IController"/> implementation.</returns>
        IController IControllerTree.GetController(int id)
        {
            return GetController(id);
        }

        /// <summary>
        /// Gets all controller instances in this tree as a collection.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="IController"/> instances.</returns>
        public IReadOnlyCollection<IController> GetAllControllers()
        {
            if (RootController == null)
                return new IController[0];
            ICollection<IController> controllersToReturn = new List<IController>();
            RootController.VisitAllNodes(c => c.ChildsCollection, controllersToReturn.Add);
            return controllersToReturn.ToReadOnly();
        }

        /// <summary>
        /// Gets the controller with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the desired controller.</param>
        /// <exception cref="InvalidOperationException">
        /// Throws an invalid operation exception if the desired controller does no exist.
        /// </exception>
        /// <returns>An <see cref="IController"/> implementation.</returns>
        public Controller GetController(int id)
        {
            Controller desiredController = TryGetController(id);
            if (desiredController == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get controller with id '{0}' on this plc information manager.", id)
                    .AppendLine()
                    .AppendLine("Unfortunately this instance did not contain that controller.")
                    .AppendFormat("The current root controller is called '{0}'", RootController.Name).AppendLine().ToString());
            return desiredController;
        }

        /// <summary>
        /// Gets the controller with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the desired controller.</param>
        /// <returns>An <see cref="IController"/> implementation.</returns>
        IController IControllerTree.TryGetController(int id)
        {
            return TryGetController(id);
        }

        /// <summary>
        /// Gets the controller with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the desired controller.</param>
        /// <returns>A <see cref="Controller"/> instance.</returns>
        public Controller TryGetController(int id)
        {
            Controller controller = null;
            RootController.VisitAndAbortOnSuccess(c => c.ChildsCollection, c => c.Id == id, c => controller = c);
            return controller;
        }

        /// <summary>
        /// Gets all <see cref="IAlarm"/> instances assigned to controller in this tree.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IAlarm"/> implementations.</returns>
        public IEnumerable<IAlarm> GetAllAlarms()
        {
            var alarms = new List<IAlarm>();

            lock (_lockObject)
            {
                RootController.VisitAllNodes(c => c.ChildsCollection, c => alarms.AddRange(c.Alarms)); 
            }

            return alarms;
        }

        /// <summary>
        /// Sends the reset alarms command to PLC.
        /// </summary>
        public void AcknowledgeAlarms()
        {
            RootController.ResetModuleAlarms();
        }
        
        /// <summary>
        /// Updates the alarms in any controller. Compares the alarms of all controllers with
        /// <paramref name="currentPlcAlarms"/>. Removes alarms from controllers that doesn't exist
        /// in the <paramref name="currentPlcAlarms"/> anymore.
        /// Add alarms from <paramref name="currentPlcAlarms"/> to controllers that not already were added.
        /// </summary>
        /// <param name="currentPlcAlarms">The current pending alarms from PLC.</param>
        public void UpdateAlarms(IEnumerable<Alarm> currentPlcAlarms)
        {
            List<Alarm> currentAlarmsList = currentPlcAlarms.ToList();
            List<Alarm> removedAlarms, addedAlarms, changedAlarms;
            
            lock (_lockObject)
            {
                // iterate through all controllers and get the alarms
                var curControllersByAlarmId = new Dictionary<uint, List<IController>>();
                var collectAlarmsById = new Action<Controller>(c =>
                {
                    foreach (var alarm in c.Alarms)
                    {
                        if (!curControllersByAlarmId.ContainsKey(alarm.Id))
                        {
                            var ctrlList = new List<IController> {c};
                            curControllersByAlarmId.Add(alarm.Id, ctrlList);
                        }
                        else
                        {
                            curControllersByAlarmId[alarm.Id].Add(c);
                        }
                    }
                });

                RootController.VisitAllNodes(c => c.ChildsCollection, collectAlarmsById);

                // remove alarms
                removedAlarms = RemoveAlarms(currentAlarmsList, curControllersByAlarmId);

                // add or replace alarms
                AddOrReplaceAlarms(currentAlarmsList, out addedAlarms, out changedAlarms); 
            }

            // raise event
            if (addedAlarms.Count > 0)
            {
                AlarmsAdded(addedAlarms);
            }

            // raise event
            if (removedAlarms.Count > 0)
            {
                AlarmsRemoved(removedAlarms);
            }

            // raise event
            if (changedAlarms.Count > 0)
            {
                AlarmsChanged(changedAlarms);
            }
        }

        private List<Alarm> RemoveAlarms(IEnumerable<Alarm> curPlcAlarms, Dictionary<uint, List<IController>> curControllersByAlarmId)
        {
            var removedAlarms = new List<Alarm>();

            // determine the alarms that are not active anymore
            var alarmIdsToDelete = curControllersByAlarmId.Keys.Except(curPlcAlarms.Select(alarm => alarm.Id));

            // loop throguh deleted alarms.
            foreach (var alarmIdToDelete in alarmIdsToDelete)
            {
                // remove the alarm from the controllers
                foreach (var controller in curControllersByAlarmId[alarmIdToDelete])
                {
                    Alarm removedAlarm;
                    if (controller.TryRemoveAlarm(alarmIdToDelete, out removedAlarm))
                        removedAlarms.Add(removedAlarm);
                }
            }

            return removedAlarms;
        }

        private void AddOrReplaceAlarms(IEnumerable<Alarm> curPlcAlarms, out List<Alarm> addedAlarms, out List<Alarm> changedAlarms)
        {
            var addedAlarmsTmp = new List<Alarm>();
            var changedAlarmsTmp = new List<Alarm>();

            foreach (IGrouping<short, Alarm> alarmsBySource in curPlcAlarms.GroupBy(a => a.SourceControllerId))
            {
                // controller ID=0 doesn't exist --> so we don't have to search the controller tree
                if (alarmsBySource.Key != 0)
                {
                    IGrouping<short, Alarm> source = alarmsBySource;
                    RootController.VisitAndAbortOnSuccess(c => c.ChildsCollection, c => c.Id == source.Key, c =>
                    {
                        foreach (var alarm in source)
                        {
                            if (alarm.Id != INVALID_ALARMID)
                            {
                                if (c.TryAddAlarm(alarm))
                                {
                                    addedAlarmsTmp.Add(alarm);
                                }
                                else if (c.TryReplaceAlarm(alarm))
                                {
                                    changedAlarmsTmp.Add(alarm);
                                }
                            }
                        }
                    });
                }
            }

            addedAlarms = new List<Alarm>(addedAlarmsTmp);
            changedAlarms = new List<Alarm>(changedAlarmsTmp);
        }

        /// <summary>
        /// Gets all overall information tags of this controller tree.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        public IReadOnlyCollection<Tag> GetOverallInformationTags()
        {
            var overallInformationTags = new List<Tag>();
            RootController.VisitAllNodes(
                c => c.ChildsCollection,
                c => overallInformationTags.AddRange(c.GetOverallInformationTags()));
            return overallInformationTags.ToReadOnly();
        }
    }
}
