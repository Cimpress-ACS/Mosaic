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


namespace VP.FF.PT.Common.PlcCommunication
{
    public enum DataStateEnum : short
    {
        /// <summary>
        /// Some data has been written to the data channel.
        /// </summary>
        DataWritten = 1,
        
        /// <summary>
        /// Acknowledge data received and processed. Channel is now free for next data package.
        /// </summary>
        DataChannelFree = -1,

        /// <summary>
        /// Clear the Fifo buffer that feeds the datachannel in the PLC
        /// </summary>
        ClearFifoBuffer = -98,

        /// <summary>
        /// Data received but invalid. Request send again.
        /// </summary>
        InvalidDataReceived = -99,
        
        /// <summary>
        /// Something is wrong, can't keep up. Error state. 
        /// </summary>
        InvalidDataReceivedError = -100
    }
}
