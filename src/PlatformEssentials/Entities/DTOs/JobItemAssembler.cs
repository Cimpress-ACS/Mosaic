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
    /// Assembler for <see cref="JobItem"/> and <see cref="JobItemDTO"/>.
    /// </summary>
    public static class JobItemAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="JobItemDTO"/> to an instance of <see cref="JobItem"/>.
        /// </summary>
        /// <param name="dto"><see cref="JobItemDTO"/> to convert.</param>
        public static JobItem ToEntity(this JobItemDTO dto)
        {
            if (dto == null) return null;

            var entity = new JobItem();

            entity.Id = dto.Id;
            entity.State = dto.State;
            entity.FailedReason = dto.FailedReason;
            entity.ItemHostId = dto.ItemHostId;

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="JobItem"/> to an instance of <see cref="JobItemDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="JobItem"/> to convert.</param>
        public static JobItemDTO ToDTO(this JobItem entity)
        {
            if (entity == null) return null;

            var dto = new JobItemDTO();

            dto.Id = entity.Id;
            dto.State = entity.State;
            dto.FailedReason = entity.FailedReason;
            dto.ItemHostId = entity.ItemHostId;

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="JobItemDTO"/> to an instance of <see cref="JobItem"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<JobItem> ToEntities(this IEnumerable<JobItemDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="JobItem"/> to an instance of <see cref="JobItemDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<JobItemDTO> ToDTOs(this IEnumerable<JobItem> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }
    }
}
