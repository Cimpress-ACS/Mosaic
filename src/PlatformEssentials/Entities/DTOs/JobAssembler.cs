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
    /// Assembler for <see cref="Job"/> and <see cref="JobDTO"/>.
    /// </summary>
    public static class JobAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="JobDTO"/> to an instance of <see cref="Job"/>.
        /// </summary>
        /// <param name="dto"><see cref="JobDTO"/> to convert.</param>
        public static Job ToEntity(this JobDTO dto)
        {
            if (dto == null) return null;

            var entity = new Job();

            entity.Id = dto.Id;
            entity.JobId = dto.JobId;
            entity.JobItems = dto.JobItems.ToEntities();

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="Job"/> to an instance of <see cref="JobDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="Job"/> to convert.</param>
        public static JobDTO ToDTO(this Job entity)
        {
            if (entity == null) return null;

            var dto = new JobDTO();

            dto.Id = entity.Id;
            dto.JobId = entity.JobId;
            dto.JobItems = entity.JobItems.ToDTOs();

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="JobDTO"/> to an instance of <see cref="Job"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<Job> ToEntities(this IEnumerable<JobDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="Job"/> to an instance of <see cref="JobDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<JobDTO> ToDTOs(this IEnumerable<Job> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }
    }
}
