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


namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Represents the mode of a PLC state machine controller.
    /// </summary>
    public enum ControllerMode : ushort
    {
        /// <summary>
        /// 
        /// </summary>
        Initialized = 0,

        /// <summary>
        /// State machine works only with internal commands. External commands will be ignored.
        /// This is the normal working mode of a state machine.
        /// </summary>
        Auto = 10,

        /// <summary>
        /// 
        /// </summary>
        Semi = 11,

        /// <summary>
        /// State machine works in manual mode and accepts external commands.
        /// </summary>
        Manual = 12
    }
}
