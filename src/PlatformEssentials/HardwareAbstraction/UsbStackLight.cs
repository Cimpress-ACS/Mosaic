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
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction
{
    // exported as shared (default) for now because multiple USB stacklights are not supported yet
    [Export(typeof(IStackLight))]
    public class UsbStackLight : ModuleEquipment, IStackLight
    {
        private readonly ILogger _logger;
        private readonly string _dllVersionString;

        /* TODO: buzzer not implemented yet
        // Pitch of Buzzer 
        const int BUZ_PITCH_OFF = 20;
        const int BUZ_PITCH1 = 21;
        const int BUZ_PITCH2 = 22;
        const int BUZ_PITCH3 = 23;
        const int BUZ_PITCH4 = 24;
        const int BUZ_PITCH5 = 25;
        const int BUZ_PITCH6 = 26;
        const int BUZ_PITCH7 = 27;
        const int BUZ_PITCH8 = 28;
        const int BUZ_PITCH9 = 29;
        const int BUZ_PITCH10 = 30;
        const int BUZ_PITCH11 = 31;
        const int BUZ_PITCH12 = 32;
        const int BUZ_PITCH13 = 33;
        const int BUZ_PITCH_DFLT = 59;
        */

        /* Connection */
        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int UPT_Open();

        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void UPT_Close();

        /* LED Conrtol */
        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int UPT_SetLight(byte color, byte ledState);

        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int UPT_SetTower(byte red, byte yel, byte grn, byte blu, byte clr);

        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int UPT_Reset();

        /* Buzzer Control */
        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int UPT_SetBuz(byte buzState, byte limit);

        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int UPT_SetBuzEx(byte buzState, byte limit, byte pitch1, byte pitch2);

        /* Debugging */
        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern ushort UPT_GetFirmVer();

        [DllImport("USB_PAT_Tower.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern ushort UPT_GetDllVer();

        [ImportingConstructor]
        public UsbStackLight(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            EquipmentName = GetType().Name;

            // DLL version acquisition
            var dllVersion = UPT_GetDllVer();
            _dllVersionString = "DLL ver : " + Convert.ToString(GetMajorVersion(dllVersion)) + "." + Convert.ToString(GetMinorVersion(dllVersion)) + "." + Convert.ToString(GetRevision(dllVersion)) + "\r\n";

            // connect
            var status = UPT_Open();
            if (status != 0)
            {
                _logger.ErrorFormat("Failed to open connection to UsbStackLight. Dll version: {0} Msg: {1}", _dllVersionString, GetErrorMessage(status));
                EquipmentState = "ERROR" + GetErrorMessage(status);
                return;
            }

            // Firmware version acquisition
            var firmwareVer = UPT_GetFirmVer();
            var firmwareVersion = "Firm ver : " + Convert.ToString(GetMajorVersion(firmwareVer)) + "." + Convert.ToString(GetMinorVersion(firmwareVer)) + "." + Convert.ToString(GetRevision(firmwareVer)) + "\r\n";

            _logger.InfoFormat("connected to UsbStackLight with dll version {0} and firmware version {1}", _dllVersionString, firmwareVersion);
            EquipmentState = "connected";
        }

        ~UsbStackLight()
        {
            UPT_Reset();
            UPT_Close();
        }

        public void IndicateRun()
        {
            var result = UPT_SetTower(
                (byte) StacklightPattern.Off,
                (byte) StacklightPattern.Off,
                (byte) StacklightPattern.On,
                (byte) StacklightPattern.Off,
                (byte) StacklightPattern.Off);

            if (result != 0)
            {
                _logger.ErrorFormat("failed to indicate stacklight RUN. Dll version: {0} Msg: {1}", _dllVersionString, GetErrorMessage(result));
                EquipmentState = "ERROR " + GetErrorMessage(result);
            }
        }

        public void IndicateStandby()
        {
            var result = UPT_SetTower(
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.BlinkSlow,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off);

            if (result != 0)
            {
                _logger.ErrorFormat("failed to indicate stacklight STANDBY. Dll version: {0} Msg: {1}", _dllVersionString, GetErrorMessage(result));
                EquipmentState = "ERROR " + GetErrorMessage(result);
            }
        }

        public void IndicateWarning()
        {
            var result = UPT_SetTower(
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.On,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off);

            if (result != 0)
            {
                _logger.ErrorFormat("failed to indicate stacklight WARNING. Dll version: {0} Msg: {1}", _dllVersionString, GetErrorMessage(result));
                EquipmentState = "ERROR " + GetErrorMessage(result);
            }
        }

        public void IndicateError()
        {
            var result = UPT_SetTower(
                (byte)StacklightPattern.On,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off,
                (byte)StacklightPattern.Off);

            if (result != 0)
            {
                _logger.ErrorFormat("failed to indicate stacklight ERROR. Dll version: {0} Msg: {1}", _dllVersionString, GetErrorMessage(result));
                EquipmentState = "ERROR " + GetErrorMessage(result);
            }
        }

        public void ControlLamp(StacklightColor color, StacklightPattern pattern)
        {
            var result = UPT_SetLight((byte)color, (byte)pattern);

            if (result != 0)
            {
                _logger.ErrorFormat("failed to control stacklight color: {0} pattern: {1}. Dll version: {2} Msg: {3}", color, pattern, _dllVersionString, GetErrorMessage(result));
                EquipmentState = "ERROR " + GetErrorMessage(result);
            }
        }

        private string GetErrorMessage(int result)
        {
            string message;

            switch (result)
            {
                case -1:
                    message = "ERR_NOEXIST";
                    break;
                case -2:
                    message = "ERR_LOCKED";
                    break;
                case -3:
                    message = "ERR_CONNECTION";
                    break;
                case -4:
                    message = "ERR_PARAM";
                    break;
                case -5:
                    message = "ERR_TRANSFAIL_EVNT";
                    break;
                case -6:
                    message = "ERR_TRANSFAIL_TMOUT";
                    break;
                case -7:
                    message = "ERR_TRANSFAIL_SEND";
                    break;
                case -8:
                    message = "ERR_DLL_LINK";
                    break;
                default:
                    message = "<unknown error code>";
                    break;
            }

            return message;
        }

        private byte GetMajorVersion(ushort ver)
        {
            return (byte)((ver >> 8) & 0xFF);
        }

        private byte GetMinorVersion(ushort ver)
        {
            return (byte)((ver >> 4) & 0x0F);
        }

        private byte GetRevision(ushort ver)
        {
            return (byte)(ver & 0x0F);
        }
    }
}
