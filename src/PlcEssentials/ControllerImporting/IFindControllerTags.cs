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


using System.Collections.Generic;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials.ControllerImporting
{
    /// <summary>
    /// An implementer of <see cref="IFindControllerTags"/> is capable of importing controller tags.
    /// </summary>
    public interface IFindControllerTags
    {
        /// <summary>
        /// Initializes the instance with the specified items.
        /// </summary>
        /// <param name="tagListener">The tag listener this instance uses to read tags.</param>
        /// <param name="address">The address under which the tags can be found.</param>
        /// <param name="port">The port under which the the tags can be found.</param>
        void Initialize(ITagListener tagListener, string address, int port = 0);

        /// <summary>
        /// Imports all controller tags found on the tagImporter.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IControllerTag"/> instances.</returns>
        IReadOnlyCollection<IControllerTag> FindControllerTags();
    }
}
