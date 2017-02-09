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
    /// Assembler for <see cref="PlatformItem"/> and <see cref="PlatformItemDTO"/>.
    /// </summary>
    public static class PlatformItemAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="PlatformItemDTO"/> to an instance of <see cref="PlatformItem"/>.
        /// </summary>
        /// <param name="dto"><see cref="PlatformItemDTO"/> to convert.</param>
        public static PlatformItem ToEntity(this PlatformItemDTO dto)
        {
            if (dto == null) return null;

            PlatformItem entity = new PlatformItem();

            entity.Id = dto.Id;
            entity.ItemId = dto.ItemId;
            entity.DetectedCount = dto.DetectedCount;
            entity.DetectedInModuleCount = dto.DetectedInModuleCount;
            entity.LastDetectionTime = dto.LastDetectionTime;
            entity.LogHistory = dto.LogHistory;

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="PlatformItem"/> to an instance of <see cref="PlatformItemDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="PlatformItem"/> to convert.</param>
        public static PlatformItemDTO ToDTO(this PlatformItem entity)
        {
            if (entity == null) return null;

            PlatformItemDTO dto = new PlatformItemDTO();

            dto.Id = entity.Id;
            dto.ItemId = entity.ItemId;
            dto.DetectedCount = entity.DetectedCount;
            dto.DetectedInModuleCount = entity.DetectedInModuleCount;
            dto.LastDetectionTime = entity.LastDetectionTime;
            dto.LogHistory = entity.LogHistory;

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="PlatformItemDTO"/> to an instance of <see cref="PlatformItem"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<PlatformItem> ToEntities(this IEnumerable<PlatformItemDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="PlatformItem"/> to an instance of <see cref="PlatformItemDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<PlatformItemDTO> ToDTOs(this IEnumerable<PlatformItem> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }

    }
}
