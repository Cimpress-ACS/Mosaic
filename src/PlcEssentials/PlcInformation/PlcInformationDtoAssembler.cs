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
using System.ComponentModel.Composition;
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Assembling;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials.PlcInformation.DTOs;

namespace VP.FF.PT.Common.PlcEssentials.PlcInformation
{
    [Export(typeof(PlcInformationDtoAssembler))]
    public class PlcInformationDtoAssembler
    {
        private readonly IAssemble<IEnumerable<Tag>, IEnumerable<TagDTO>> _tagDtosAssembler;

        public PlcInformationDtoAssembler()
        {
            _tagDtosAssembler = new NullFilteringEnumerableAssembler<Tag, TagDTO>(new TagDtoAssembler());
        }

        public virtual ControllerDTO AssembleControllerDto(IController controller, bool recursive = false)
        {
            if (controller == null)
                return null;
            var dto = new ControllerDTO
            {
                ActiveAlarm = GetActiveAlarmText(controller),
                ActualValues = AssembleTagDtos(controller.ActualValues).ToList(),
                Children = new List<ControllerDTO>(0),
                Commands = AssembleCommandDtos(controller.Commands).ToList(),
                ControllerState = GetActiveAlarmType(controller),
                Configurations = AssembleTagDtos(controller.Configurations).ToList(),
                CurrentState = controller.CurrentState,
                CurrentSubState = controller.CurrentSubState,
                EnableForcing = controller.EnableForcing,
                Id = controller.Id,
                Inputs = AssembleTagDtos(controller.Inputs).ToList(),
                IsEnabled = controller.IsEnabled,
                IsSimulation = controller.IsSimulation,
                Mode = (DTOs.ControllerMode)controller.ControllerMode,
                Name = controller.Name,
                Outputs = AssembleTagDtos(controller.Outputs).ToList(),
                Parameters = AssembleTagDtos(controller.Parameters).ToList(),
                Type = controller.Type,
                PlcControllerPath = controller.PlcInternalTreePath,
                FullName = controller.FullName
                
            };
            if (recursive)
                dto.Children = AssembleControllerDtos(controller.Childs).ToList();
            return dto;
        }
        
        private static string GetActiveAlarmText(IController controller)
        {
            IAlarm activeAlarm = controller.ActiveAlarm;
            if (activeAlarm == null)
                return string.Empty;
            return activeAlarm.Text;
        }

        private static DTOs.AlarmType GetActiveAlarmType(IController controller)
        {
            IAlarm activeAlarm = controller.ActiveAlarm;
            if (activeAlarm == null)
                return DTOs.AlarmType.Normal;
            switch (activeAlarm.AlarmType)
            {
                case AlarmType.None:
                case AlarmType.Info:
                    return DTOs.AlarmType.Normal;
                case AlarmType.Warning:
                    return DTOs.AlarmType.Warning;
                default:
                    return DTOs.AlarmType.Error;
            }
        }

        public virtual IEnumerable<ControllerDTO> AssembleControllerDtos(IEnumerable<IController> controllers)
        {
            if (controllers == null)
                return new List<ControllerDTO>();
            return controllers.Where(x => x != null).Select(c => AssembleControllerDto(c, recursive: true)).ToList();
        }

        public virtual CommandDTO AssembleCommandDto(ICommand command)
        {
            if (command == null)
                return null;
            return new CommandDTO { Name = command.Name, OptionalText = command.Comment };
        }

        public virtual IEnumerable<CommandDTO> AssembleCommandDtos(IEnumerable<ICommand> commands)
        {
            if (commands == null)
                return new List<CommandDTO>();
            return commands.Where(x => x != null).Select(AssembleCommandDto);
        }

        private IEnumerable<TagDTO> AssembleTagDtos(IEnumerable<Tag> tags)
        {
            return _tagDtosAssembler.Assemble(tags);
        }
    }
}
