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
using System.Linq;

namespace VP.FF.PT.Common.PlatformEssentials.Entities.DTOs
{
    /// <summary>
    /// Assembler for <see cref="PlatformModule"/> and <see cref="PlatformModuleDTO"/>.
    /// </summary>
    public static class PlatformModuleAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="PlatformModuleDTO"/> to an instance of <see cref="PlatformModule"/>.
        /// </summary>
        /// <param name="dto"><see cref="PlatformModuleDTO"/> to convert.</param>
        public static PlatformModule ToEntity(this PlatformModuleDTO dto)
        {
            if (dto == null) return null;

            var entity = new PlatformModule();

            entity.Name = dto.Name;
            entity.IsEnabled = dto.IsEnabled;
            entity.AdsPort = dto.AdsPort;
            entity.PathRootController = dto.PathRootController;
            entity.MaxCapacity = dto.MaxCapacity;

            entity.State = dto.State;
            entity.SubState = dto.SubState;
            entity.StreamType = dto.StreamType;
            entity.Entities.PlatformItems = dto.PlatformItems.ToEntities();

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="PlatformModule"/> to an instance of <see cref="PlatformModuleDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="PlatformModule"/> to convert.</param>
        public static PlatformModuleDTO ToDTO(this PlatformModule entity)
        {
            if (entity == null) return null;

            var dto = new PlatformModuleDTO();

            dto.Name = entity.Name;
            dto.IsEnabled = entity.IsEnabled;
            dto.AdsPort = entity.AdsPort;
            dto.PathRootController = entity.PathRootController;
            dto.MaxCapacity = entity.MaxCapacity;

            dto.State = entity.State;
            dto.SubState = entity.SubState;
            dto.StreamType = entity.StreamType;

            if (entity.Entities != null)
            {
                dto.PlatformItems = entity.Entities.PlatformItems.ToList().ToDTOs();
            }
            else
            {
                dto.PlatformItems = new List<PlatformItemDTO>();
            }

            dto.Type = entity.ModuleTypeId;

            dto.MaxCapacity = entity.LimitItemCount > 0 ? entity.LimitItemCount : entity.MaxCapacity;

            dto.HasErrors = entity.AlarmManager.HasErrors;
            dto.HasWarnings = entity.AlarmManager.HasWarnings;

            // set the most important alarm, first error, if that doesn't exist warning, and if that doesn't exist to an empty string
            var alarm = entity.AlarmManager.CurrentAlarms.FirstOrDefault(a => a.Type == AlarmType.Error) ??
                        entity.AlarmManager.CurrentAlarms.FirstOrDefault(a => a.Type == AlarmType.Warning);
            dto.MostImportantAlarmText = alarm != null ? alarm.Message : string.Empty;

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="PlatformModuleDTO"/> to an instance of <see cref="PlatformModule"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<PlatformModule> ToEntities(this IEnumerable<PlatformModuleDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="PlatformModule"/> to an instance of <see cref="PlatformModuleDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<PlatformModuleDTO> ToDTOs(this IEnumerable<PlatformModule> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }

    }
}
