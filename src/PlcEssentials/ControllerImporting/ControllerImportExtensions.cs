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
using VP.FF.PT.Common.PlcEssentials;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    public static class ControllerImportExtensions
    {
        public static IEnumerable<Command> Commands(this CtrlJob job)
        {
 //           return job.commands;
            yield return job.command0;
            yield return job.command1;
            yield return job.command2;
            yield return job.command3;
            yield return job.command4;
            yield return job.command5;
            yield return job.command6;
            yield return job.command7;
            yield return job.command8;
            yield return job.command9;
            yield return job.command10;
            yield return job.command11;
            yield return job.command12;
            yield return job.command13;
            yield return job.command14;
            yield return job.command15;
        }
    }
}
