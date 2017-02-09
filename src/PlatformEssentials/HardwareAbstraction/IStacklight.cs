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


namespace VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction
{
    /// <summary>
    /// Controls a signal tower to indicate the state of a module or the whole platform.
    /// </summary>
    public interface IStackLight : IModuleEquipment
    {
        /// <summary>
        /// Indicates the run state, green lamp permanent on.
        /// </summary>
        void IndicateRun();

        /// <summary>
        /// Indicates the standby state, green lamp blinks.
        /// </summary>
        void IndicateStandby();

        /// <summary>
        /// Indicates the warning state, yellow lamp permanent on.
        /// </summary>
        void IndicateWarning();

        /// <summary>
        /// Indicates the error state, red lamp permanent on.
        /// </summary>
        void IndicateError();

        /// <summary>
        /// Controls a single lamp.
        /// </summary>
        /// <param name="color">The stacklight color to control if existing.</param>
        /// <param name="pattern">The light pattern of the color.</param>
        void ControlLamp(StacklightColor color, StacklightPattern pattern);
    }

    public enum StacklightColor : byte
    {
        Red = 10,
        Yellow = 11,
        Green = 12,
        Blue = 13,
        White = 14
    }

    public enum StacklightPattern : byte
    {
        Off = 0,
        On = 1,
        BlinkFast = 2,
        BlinkSlow = 3,
        DoubleBlink = 4,
        BlinkFastAndShort = 5
    }
}
