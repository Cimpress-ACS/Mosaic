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


namespace VP.FF.PT.Common.GuiEssentials.Wcf
{
    /// <summary>
    /// An implementer of <see cref="ICommunicationObjectFactory{TService}"/> 
    /// provides funcationalities to create a communication object capable to
    /// communicate with a <see cref="TService"/> instance.
    /// </summary>
    /// <typeparam name="TService">The type of the service the created object can communicate with.</typeparam>
    public interface ICommunicationObjectFactory<out TService>
    {
        /// <summary>
        /// Creates a communication object communicating with a <see cref="TService"/> instance.
        /// </summary>
        /// <returns>A communication object implementing the <see cref="TService"/> interface of the service it communicates with.</returns>
        TService CreateCommunicationObject();
    }
}
