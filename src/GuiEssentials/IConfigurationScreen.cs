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


using System.ComponentModel.Composition;

namespace VP.FF.PT.Common.GuiEssentials
{
    [InheritedExport]
    public interface IConfigurationScreen
    {
        string DisplayName { get; }

        /// <summary>
        /// Creates WCF client and establish connection (optional).
        /// Creates other critical/potential error stuff here. Do NOT use the constructor for this.
        /// The application will continue to run, even if an exception is thrown in the initialize phase, but just the failed module disabled (and log entries).
        /// The application will crash if an exception is thrown in the constructor, so use Initialize()!
        /// </summary>
        void Initialize();

        /// <summary>
        /// Will be called on application shutdown, the configuration screen should close WCF connections.
        /// </summary>
        void Shutdown();
    }
}
