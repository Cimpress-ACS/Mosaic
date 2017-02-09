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


using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    public static class SimulatedPlcBehaviorManagerExtensions
    {
        public static void AddDefaultStartStopBehavior(this ISimulatedPlcBehaviorManager manager, string pathRootController, ISimulatedPlcBehaviorManager plcBehaviorManager, int taktDelay = 6)
        {
            string commandChannelTag = pathRootController + "." + NamingConventions.CommonInterfaceAutoCmdChannel;

            var stateTag = plcBehaviorManager.SearchTag(pathRootController + "." + NamingConventions.CommonInterfaceState);
            var stateNameTag = plcBehaviorManager.SearchOrCreateLooseTag(pathRootController + "." + NamingConventions.CommonInterfaceStateName);
            var subStateNameTag = plcBehaviorManager.SearchOrCreateLooseTag(pathRootController + "." + NamingConventions.CommonInterfaceSubStateName);

            stateNameTag.Value = "cSTA_OFF";
            subStateNameTag.Value = "cSTA_SUB_INITIALIZED";
            stateTag.Value = (short)StandardStates.Off;

            // START behavior
            plcBehaviorManager
                .WhenTag(commandChannelTag)
                .ValueEquals((short) StandardCommands.Run)
                .When(() => ((short) stateTag.Value) != (short) StandardStates.Run && (short) stateTag.Value != (short) StandardStates.RunBusy)
                .Then(() =>
                {
                    stateNameTag.Value = "cSTA_RUNBusy";
                    stateTag.Value = (short) StandardStates.RunBusy;
                    subStateNameTag.Value = "cSTA_SUB_SEND_GROUP_CMD";
                })
                .ThenWithDelay(taktDelay, () => subStateNameTag.Value = "cSTA_SUB_WAIT")
                .ThenWithDelay(taktDelay, () =>
                {
                    stateNameTag.Value = "cSTA_RUN";
                    stateTag.Value = (short) StandardStates.Run;
                    subStateNameTag.Value = "cSTA_SUB_FINISHED";
                })
                .CancelOnTagChange(commandChannelTag);

            // STANDBY behavior
            plcBehaviorManager
                .WhenTag(commandChannelTag)
                .ValueEquals((short)StandardCommands.Standby)
                .When(() => ((short)stateTag.Value) != (short)StandardStates.Standby)
                .Then(() =>
                {
                    stateNameTag.Value = "cSTA_STANDBYBusy";
                    stateTag.Value = (short)StandardStates.StandbyBusy;
                    subStateNameTag.Value = "cSTA_SUB_SEND_GROUP_CMD";
                })
                .ThenWithDelay(taktDelay, () => subStateNameTag.Value = "cSTA_SUB_WAIT")
                .ThenWithDelay(taktDelay, () =>
                {
                    stateNameTag.Value = "cSTA_STANDBY";
                    stateTag.Value = (short)StandardStates.Standby;
                    subStateNameTag.Value = "cSTA_SUB_FINISHED";
                })
                .CancelOnTagChange(commandChannelTag);

            // OFF behavior
            plcBehaviorManager
                .WhenTag(commandChannelTag)
                .ValueEquals((short)StandardCommands.Off)
                .When(() => ((short)stateTag.Value) != (short)StandardStates.Off)
                .Then(() =>
                {
                    stateNameTag.Value = "cSTA_STOPBusy";
                    stateTag.Value = (short)StandardStates.OffBusy;
                    subStateNameTag.Value = "cSTA_SUB_SEND_GROUP_CMD";
                })
                .ThenWithDelay(taktDelay, () => subStateNameTag.Value = "cSTA_SUB_WAIT")
                .ThenWithDelay(taktDelay, () =>
                {
                    stateNameTag.Value = "cSTA_STOP";
                    stateTag.Value = (short)StandardStates.Off;
                    subStateNameTag.Value = "cSTA_SUB_FINISHED";
                })
                .CancelOnTagChange(commandChannelTag);

            // STOP behavior
            plcBehaviorManager
                .WhenTag(commandChannelTag)
                .ValueEquals((short)StandardCommands.Stop)
                .When(() => ((short)stateTag.Value) != (short)StandardStates.Stop)
                .Then(() =>
                {
                    stateNameTag.Value = "cSTA_STOPBusy";
                    stateTag.Value = (short)StandardStates.StopBusy;
                    subStateNameTag.Value = "cSTA_SUB_SEND_GROUP_CMD";
                })
                .ThenWithDelay(taktDelay, () => subStateNameTag.Value = "cSTA_SUB_WAIT")
                .ThenWithDelay(taktDelay, () =>
                {
                    stateNameTag.Value = "cSTA_STOP";
                    stateTag.Value = (short)StandardStates.Stop;
                    subStateNameTag.Value = "cSTA_SUB_FINISHED";
                })
                .CancelOnTagChange(commandChannelTag);
        }
    }
}
