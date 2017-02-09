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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwinCAT.Ads;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    public static class BeckhoffHelper
    {
        private static readonly ITagMetaDataParser MetaDataParser = new TagMetaDataParser();

        /// <summary>
        /// Determines whether the specified twinCatClient client is connected and in running state.
        /// </summary>
        /// <param name="twinCatClient">The twin cat client.</param>
        /// <returns>
        ///   <c>true</c> if the specified twin cat client is connected; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConnected(TcAdsClient twinCatClient)
        {
            var dataStream = new AdsStream(2);

            try
            {
                twinCatClient.Read((int)AdsReservedIndexGroups.DeviceData,
                                    (int)AdsReservedIndexOffsets.DeviceDataAdsState, dataStream);

                var reader = new BinaryReader(dataStream);
                var plcState = (AdsState)reader.ReadInt16();

                if (plcState == AdsState.Run)
                    return true;

                return false;
            }
            catch (AdsErrorException)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts an ADS symbol to tag.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="adsPort">The ads port.</param>
        /// <returns></returns>
        /// <exception cref="TagException"></exception>
        public static Tag ConvertSymbolToTag(ITcAdsSymbol symbol, int adsPort)
        {
            if (symbol == null)
                return null;

            string[] names = symbol.Name.Split('.');

            string name;
            string scope;

            if (names.Length == 1)
            {
                scope = string.Empty;
                name = names[0];
            }
            else
            {
                scope = names[0];
                name = symbol.Name.Replace(scope + ".", string.Empty);
            }

            var tag = new Tag
                {
                    AdsPort = adsPort,
                    Name = name,
                    Scope = scope,
                    BitSize = symbol.Size*8,
                    DataType = symbol.Type,
                    IndexGroup = symbol.IndexGroup,
                    IndexOffset = symbol.IndexOffset
                };
            
            tag.MetaData = MetaDataParser.Parse(symbol.Comment);

            return tag;
        }

        private static readonly IDictionary<string, Func<AdsBinaryReader, int, object>> _dataTypeMapping;
        private static readonly IDictionary<int, Func<AdsBinaryReader, object>> _bitSizeMapping;

        static BeckhoffHelper()
        {
            _dataTypeMapping = new Dictionary<string, Func<AdsBinaryReader, int, object>>(StringComparer.InvariantCultureIgnoreCase);

            _dataTypeMapping.Add(IEC61131_3_DataTypes.Boolean, (r, _) => r.ReadBoolean());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.Byte, (r, _) => r.ReadByte());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.SInt, (r, _) => r.ReadSByte());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.USInt, (r, _) => r.ReadByte());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.UInt, (r, _) => r.ReadUInt16());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.Int, (r, _) => r.ReadInt16());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.DInt, (r, _) => r.ReadInt32());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.LInt, (r, _) => r.ReadInt64());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.String, (r, b) => r.ReadPlcString(b / 8));
            _dataTypeMapping.Add(IEC61131_3_DataTypes.Real, (r, _) => r.ReadSingle());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.LReal, (r, _) => r.ReadDouble());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.Word, (r, _) => r.ReadUInt16());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.DWord, (r, _) => r.ReadUInt32());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.LWord, (r, _) => r.ReadUInt64());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.ULInt, (r, _) => r.ReadUInt64());
            _dataTypeMapping.Add(IEC61131_3_DataTypes.UDInt, (r, _) => r.ReadUInt32());

            _bitSizeMapping = new Dictionary<int, Func<AdsBinaryReader, object>>();

            _bitSizeMapping.Add(1, r => r.ReadBoolean());
            _bitSizeMapping.Add(8, r => r.ReadByte());
            _bitSizeMapping.Add(16, r => r.ReadUInt16());
            _bitSizeMapping.Add(32, r => r.ReadUInt32());
            _bitSizeMapping.Add(64, r => r.ReadUInt64());
        }

        public static object ReadDataType(string dataType, int bitSize, AdsBinaryReader binaryReader)
        {
            // standard data formats
            Func<AdsBinaryReader, int, object> dataTypeFunc;
            if (_dataTypeMapping.TryGetValue(dataType, out dataTypeFunc))
            {
                return dataTypeFunc(binaryReader, bitSize);
            }

            // enum
            if (IsEnum(dataType, bitSize))
            {
                return binaryReader.ReadInt16();
            }

            // any "STRING(14)" types with variable length
            if (dataType.StartsWith(IEC61131_3_DataTypes.String))
            {
                return binaryReader.ReadPlcString(bitSize/8);
            }

            // default to simply use the bit size
            Func<AdsBinaryReader, object> intFunc;
            if (_bitSizeMapping.TryGetValue(bitSize, out intFunc))
            {
                return intFunc(binaryReader);
            }
            
            // cannot convert it, default to return null
            return null;
        }

        public static bool IsEnum(string dataType, int bitSize)
        {
            if (bitSize != 16)
            {
                return false;
            }

            return dataType.Split('.').Last().StartsWith("E_");
        }
    }
}
