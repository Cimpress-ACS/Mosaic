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
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcEssentials.ControllerImporting
{
    /// <summary>
    /// An implementer of <see cref="ICreateController"/> is capable of
    /// <see cref="Controller"/> instances.
    /// </summary>
    public interface ICreateController
    {
        /// <summary>
        /// Creates a new <see cref="Controller"/> instance.
        /// </summary>
        /// <param name="tagController">The tag controller used by the created controller.</param>
        /// <param name="controllerTag">The tag, the new controller is dependend on.</param>
        /// <param name="tagListener"></param>
        /// <returns>A new <see cref="Controller"/> instance.</returns>
        Controller Create(ITagController tagController, IControllerTag controllerTag, ITagListener tagListener);

        /// <summary>
        /// Creates a new <see cref="Controller"/> instance.
        /// </summary>
        /// <param name="tagController">The tag controller used by the created controller.</param>
        /// <param name="controllerTag">The tag, the new controller is dependend on.</param>
        /// <param name="userDefinedInterfaces">User defined interfaces</param>
        /// <param name="tagListener">A tag listener instance</param>
        /// <returns>A new <see cref="Controller"/> instance.</returns>
        Controller Create(
            ITagController tagController, 
            IControllerTag controllerTag, 
            IList<string> userDefinedInterfaces,
            ITagListener tagListener);
    }
}
