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
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using TwinCAT.Ads;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// Beckhoff technology is able to download variable symbol informations using ADS communication.
    /// This way we can import all Tags directly from PLC.
    /// </summary>
    [Export(typeof(ITagImporter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BeckhoffOnlineTagImporter : ITagImporter
    {
        private readonly ILogger _logger;
        private string _address;
        private int _port;
        private TcAdsClient _twinCatClient;
        private TcAdsSymbolInfoLoader _symbolLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffOnlineTagImporter"/> class.
        /// </summary>
        /// <param name="adsAddress">The ads address.</param>
        /// <param name="adsPort">The ads port.</param>
        public BeckhoffOnlineTagImporter(string adsAddress, int adsPort)
            : this (new Log4NetLogger())
        {
            _address = adsAddress;
            _port = adsPort;
            InitializieConnection(adsAddress, adsPort);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffOnlineTagImporter"/> class.
        /// </summary>
        /// <param name="logger">The logger</param>
        [ImportingConstructor]
        public BeckhoffOnlineTagImporter(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
        }

        /// <summary>
        /// Initializes this tag importer to communicate with the plc on the specified address and port.
        /// </summary>
        /// <param name="address">The address the plc can found on.</param>
        /// <param name="port">The port in cas of Beckhoff implementation.</param>
        public void Initialize(string address, int port = 0)
        {
            _address = address;
            _port = port;
            InitializieConnection(_address, _port);
        }

        /// <summary>
        /// Imports all tags under the preconfigured address.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        public IReadOnlyCollection<Tag> ImportAllTags()
        {
            return ImportTags(_address, _port).ToReadOnly();
        }


        /// <summary>
        /// Imports tags recursively from the specified <paramref name="startTag"/>.
        /// </summary>
        /// <param name="startTag">The path of the tag to start the import from.</param>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        public IReadOnlyCollection<Tag> ImportTags(string startTag)
        {
            return ImportTags(startTag, _address, _port).ToReadOnly();
        }

        /// <summary>
        /// Imports the tags directly from PLC.
        /// </summary>
        /// <param name="adsAddress">The ads address to PLC. Port is hardcoded to 851.</param>
        /// <param name="adsPort">The ads port.</param>
        /// <returns>
        /// List of imported tags.
        /// </returns>
        /// <exception cref="PlcCommunicationException">Cannot import tags online. Connection to PLC failed.</exception>
        /// <exception cref="System.NotImplementedException">Cannot resolve tag scope and name. FullName contains more than one . operator!</exception>
        /// <exception cref="AdsErrorException">When connection to PLC is not possible an exception will be thrown.</exception>
        public ICollection<Tag> ImportTags(string adsAddress, int adsPort)
        {
            _logger.Debug(string.Format("Importing all tags recursively from the plc under '{0}:{1}'", adsAddress, adsPort));
            TcAdsSymbolInfo symbol;

            try
            {
                InitializieConnection(adsAddress, adsPort);
                //int count = _symbolLoader.GetSymbolCount(false);
                symbol = _symbolLoader.GetFirstSymbol(false);
            }
            catch (AdsErrorException e)
            {
                throw new PlcCommunicationException("Cannot import tags online. Connection to PLC failed.", adsAddress, adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
            }

            var tmpRootTag = new Tag();

            ImportChildTagsRecursively(tmpRootTag, symbol, adsPort);

            return tmpRootTag.Childs.ToList(); 
        }

        /// <summary>
        /// Imports the tags recursively from startTag.
        /// </summary>
        /// <param name="startTag">The startTag address indicates the start position for import. Must include scope and full nested name.</param>
        /// <param name="adsAddress">The ads address.</param>
        /// <param name="adsPort">The ads port.</param>
        /// <returns>
        /// Collection of imported tags.
        /// </returns>
        /// <exception cref="PlcCommunicationException">Cannot import tags online. Connection to PLC failed.</exception>
        public ICollection<Tag> ImportTags(string startTag, string adsAddress, int adsPort)
        {
            _logger.Debug(string.Format("Importing tags starting at '{0}' on plc '{1}:{2}'.", startTag, adsAddress, adsPort));
            TcAdsSymbolInfo symbol;

            try
            {
                InitializieConnection(adsAddress, adsPort);
                symbol = _symbolLoader.FindSymbol(startTag);
            }
            catch (AdsErrorException e)
            {
                throw new PlcCommunicationException("Cannot import tags online. Connection to PLC failed.", adsAddress, adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
            }

            var tmpRootTag = new Tag();

            ImportChildTagsRecursively(tmpRootTag, symbol, adsPort);

            return tmpRootTag.Childs.ToList();
        }
        
        /// <summary>
        /// Search for the startTag in the passed tagList. There is no access needed to plc.
        /// </summary>
        /// <param name="startTag">The startTag address indicates the start position for import.</param>
        /// <param name="tagList">The list which is searched for the startTag</param>
        /// <returns>
        /// Collection of imported tags.
        /// </returns>
        public ICollection<Tag> ImportTagsFromCache(string startTag, IEnumerable<Tag> tagList)
        {
            var startTags = startTag.Split('.');
            var tagTree = new List<Tag>(tagList);
            var resultTree = new List<Tag>();

            int iIdx;
            for (iIdx = 0; iIdx < startTags.Count(); iIdx++)
            {
                tagTree = tagTree.FindAll(t => t.NestedName == startTags[iIdx]);

                if (iIdx < (startTags.Count() - 1))
                {
                    foreach (var t in tagTree)
                    {
                        resultTree.AddRange(t.Childs);
                    }

                    tagTree = new List<Tag>(resultTree);
                }

                resultTree.Clear();
            }

            return tagTree.ToArray();
        }

        private void InitializieConnection(string adsAddress, int adsPort)
        {
            try
            {
                _twinCatClient = new TcAdsClient();

                _twinCatClient.Connect(adsAddress, adsPort);

                _symbolLoader = _twinCatClient.CreateSymbolInfoLoader();
            }
            catch (AdsErrorException e)
            {
                throw new PlcCommunicationException("Can't establish connection to PLC", adsAddress,
                                    adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
            }
        }

        /// <summary>
        /// Imports a tag with all its children recursively.
        /// </summary>
        /// <param name="scopedPath">The path to the tag with its scope.</param>
        /// <returns>A <see cref="Tag"/> instance.</returns>
        public Tag ImportTagRecursive(string scopedPath)
        {
//            _logger.Debug(string.Format("Importing tag with all its childs at '{0}' on plc '{1}:{2}'.", scopedPath, _address, _port));
            TcAdsSymbolInfo symbol;

            try
            {
                symbol = _symbolLoader.FindSymbol(scopedPath);
            }
            catch (AdsErrorException e)
            {
                throw new PlcCommunicationException("Cannot import tags online. Connection to PLC failed.", _address, _port.ToString(CultureInfo.InvariantCulture), string.Empty, e);
            }

            return ImportTagWithChilds(symbol, _port, new HashSet<string>());
        }

        private Tag ImportTagWithChilds(TcAdsSymbolInfo symbol, int adsPort, HashSet<string> tagHashSet)
        {                
            Tag tag = null;

            if (symbol == null)
                return tag;

            try
            {
                tag = BeckhoffHelper.ConvertSymbolToTag(symbol, adsPort);
            }
            catch (TagException e)
            {
                _logger.Warn("Cannot import tags online (will skip it): " + e);
                return tag;
            }

            // prevent circular reads (endless loop) by checking if the hashset already contains the tag
            if ((symbol.IsReference || symbol.IsPointer) && (!tag.MetaData.ReadPointer || tagHashSet.Contains(tag.GetPointerlessFullName())))
                return tag;
            
            if (tag.MetaData.ReadPointer)
                tagHashSet.Add(tag.GetPointerlessFullName());

            tag.FullName();

            TcAdsSymbolInfo subSymbol = symbol.FirstSubSymbol;
            while (subSymbol != null)
            {
                Tag subTag = ImportTagWithChilds(subSymbol, adsPort, tagHashSet);
                if (subTag != null)
                {
                    tag.Childs.Add(subTag);
                    subTag.Parent = tag;
                }
                subSymbol = subSymbol.NextSymbol;
            }
            return tag;
        }

        public Tag ImportTag(string name)
        {
            if (_twinCatClient == null)
                return null;

            return ImportTag(name, _twinCatClient.ServerPort);
        }

        public Tag ImportTag(string name, int adsPort)
        {
            _logger.Debug(string.Format("Importing tag '{0}' on port '{1}", name, adsPort));
            if (_twinCatClient == null)
                return null;

            var symbol = _twinCatClient.ReadSymbolInfo(name);

            if (symbol == null)
                return null;

            Tag result = BeckhoffHelper.ConvertSymbolToTag(symbol, adsPort);

            return result;
        }

        private void ImportChildTagsRecursively(Tag parentTag, TcAdsSymbolInfo symbol, int adsPort)
        {
            while (symbol != null)
            {
                var tag = new Tag();

                try
                {
                    tag = BeckhoffHelper.ConvertSymbolToTag(symbol, adsPort);
                    tag.Parent = parentTag;
                    parentTag.Childs.Add(tag);
                }
                catch (TagException e)
                {
                    Console.WriteLine("Cannot import tags online (will skip it): " + e);
                }

                if (symbol.SubSymbolCount > 0 && !symbol.IsPointer && !symbol.IsReference)
                {
                    ImportChildTagsRecursively(tag, symbol.FirstSubSymbol, adsPort);
                }

                symbol = symbol.NextSymbol;
            }
        }

        public void Dispose()
        {
            _twinCatClient.Dispose();
        }
    }
}
