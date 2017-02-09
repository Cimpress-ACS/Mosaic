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
    public static class AlarmAssembler
    {
        public static Alarm ToEntity(this AlarmDTO dto)
        {
            if (dto == null) return null;

            var entity = new Alarm();

            entity.Id = dto.Id;
            entity.Type =  dto.Type;
            entity.Message = dto.Message;
            entity.Timestamp = dto.Timestamp;
            entity.Source = dto.Source;

            return entity;
        }

        public static AlarmDTO ToDTO(this Alarm entity)
        {
            if (entity == null) return null;

            var dto = new AlarmDTO();

            dto.Id = entity.Id;
            dto.Type = entity.Type;
            dto.Message = entity.Message;
            dto.Timestamp = entity.Timestamp;
            dto.Source = entity.Source;

            return dto;
        }

        public static List<Alarm> ToEntities(this IEnumerable<AlarmDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => ToEntity(e)).ToList();
        }

        public static List<AlarmDTO> ToDTOs(this IEnumerable<Alarm> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => ToDTO(e)).ToList();
        }
    }
}
