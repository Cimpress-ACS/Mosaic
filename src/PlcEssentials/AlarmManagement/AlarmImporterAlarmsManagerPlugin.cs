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
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlcEssentials.AlarmManagement
{
    /// <summary>
    /// The <see cref="AlarmImporterAlarmsManagerPlugin"/> is capable of 
    /// managing the current alarms of a plc using the <see cref="IAlarmsImporter"/>.
    /// It is usually used once at import, and then exchanged with the <see cref="ControllerTreeAlarmsManagerPlugin" />.
    /// </summary>
    public class AlarmImporterAlarmsManagerPlugin : IManageCurrentAlarmsPlugin
    {
        private readonly IAlarmsImporter _alarmsImporter;

        /// <summary>
        /// Initializes a new <see cref="AlarmImporterAlarmsManagerPlugin"/> instance.
        /// </summary>
        /// <param name="alarmsImporter">The alarms importer to import the alarms from.</param>
        public AlarmImporterAlarmsManagerPlugin(IAlarmsImporter alarmsImporter)
        {
            _alarmsImporter = alarmsImporter;
            _alarmsImporter.ImportAlarms();
            AlarmAdded += _ => { };
        }

        /// <summary>
        /// Notifies subscribers about one or more added alarms to the current alarms.
        /// </summary>
        public event Action<Alarm> AlarmAdded { add { } remove { } }

        /// <summary>
        /// Gets the current alarms.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        public IEnumerable<Alarm> GetCurrentAlarms()
        {
            return _alarmsImporter.GetAllImportedAlarms()
                                  .Distinct(new AlarmByIdComparer())
                                  .Select(a => this.CopyPlcAlarmToAlarm(a, string.Format("Controller Id: {0}", a.SourceControllerId)))
                                  .Where(a => a != null)
                                  .ToReadOnly();
        }

        public void TryRemoveAlarms(IEnumerable<Alarm> alarmsToRemove)
        {
            // do nothing
        }

        public void ForceRemoveAlarms(IEnumerable<Alarm> alarmsToRemove)
        {
            // do nothing
        }

        public int ForceRemoveAlarms(string source, int alarmId)
        {
            // do nothing
            return 0;
        }

        private class AlarmByIdComparer : IEqualityComparer<IAlarm>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public bool Equals(IAlarm x, IAlarm y)
            {
                if (x == null)
                    return y == null;
                if (y == null)
                    return false;
                return x.Id == y.Id;
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(IAlarm obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
