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
using System.Text;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    /// <summary>
    /// The <see cref="AlarmByIdAndSourceComparer"/> compares alarms by
    /// their id and source, considering alarms with the same id and same source as the same alarms.
    /// </summary>
    public class AlarmByIdAndSourceComparer : IEqualityComparer<Alarm>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <see cref="Alarm"/> to compare.</param>
        /// <param name="y">The second object of type <see cref="Alarm"/> to compare.</param>
        public bool Equals(Alarm x, Alarm y)
        {
            if (x == null)
                return y == null;
            if (y == null)
                return false;
            return x.AlarmId == y.AlarmId && string.Equals(x.Source, y.Source);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <remarks>
        /// The Jon Skeet way :)
        /// </remarks>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(Alarm obj)
        {
            if (obj == null)
                throw new ArgumentNullException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get hash code of the Alarm 'null' over this '{0}'.", GetType().FullName).AppendLine()
                    .AppendLine("It is not possible to get the hash code of a null value.")
                    .AppendFormat("Please check the code invoking {0}.GetHashCode(Alarm) to ensure not passing null.", GetType().FullName)
                    .ToString());
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.AlarmId.GetHashCode();
                if (obj.Source != null)
                    hash = hash * 23 + obj.Source.GetHashCode();
                return hash;
            }
        }
    }
}
