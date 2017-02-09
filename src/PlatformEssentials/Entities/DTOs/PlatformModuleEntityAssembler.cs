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
    /// Assembler for <see cref="PlatformModuleEntity"/> and <see cref="PlatformModuleEntityDTO"/>.
    /// </summary>
    public static class PlatformModuleEntityAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="PlatformModuleEntityDTO"/> to an instance of <see cref="PlatformModuleEntity"/>.
        /// </summary>
        /// <param name="dto"><see cref="PlatformModuleEntityDTO"/> to convert.</param>
        public static PlatformModuleEntity ToEntity(this PlatformModuleEntityDTO dto)
        {
            if (dto == null) return null;

            var entity = new PlatformModuleEntity();

            entity.Id = dto.Id;
            entity.Name = dto.Name;
            entity.LimitItemCount = dto.LimitItemCount;

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="PlatformModuleEntity"/> to an instance of <see cref="PlatformModuleEntityDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="PlatformModuleEntity"/> to convert.</param>
        public static PlatformModuleEntityDTO ToDTO(this PlatformModuleEntity entity)
        {
            if (entity == null) return null;

            var dto = new PlatformModuleEntityDTO();

            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.LimitItemCount = entity.LimitItemCount;

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="PlatformModuleEntityDTO"/> to an instance of <see cref="PlatformModuleEntity"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<PlatformModuleEntity> ToEntities(this IEnumerable<PlatformModuleEntityDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="PlatformModuleEntity"/> to an instance of <see cref="PlatformModuleEntityDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<PlatformModuleEntityDTO> ToDTOs(this IEnumerable<PlatformModuleEntity> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }
    }
}
