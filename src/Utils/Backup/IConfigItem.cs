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
using System.Threading.Tasks;

namespace VP.FF.PT.Common.Utils.Backup
{
    /// <summary>
    /// Interface that represents an item to be monitored for change
    /// </summary>
    public interface IConfigItem
    {
        /// <summary>
        /// Source information / key
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// Identifier to be used when persisting/retrieving the item
        /// </summary>
        string Identifier { get; set; }

        /// <summary>
        /// True: the item will be save at start if it is not peristed yet or
        /// it changed since the last time it was persisted
        /// </summary>
        bool SaveAtStart { get; set; }

        /// <summary>
        /// True: the item will be retrieved from the file store if
        /// does not exist at startup
        /// </summary>
        bool RestoreAtStart { get; set; }
    }
}
