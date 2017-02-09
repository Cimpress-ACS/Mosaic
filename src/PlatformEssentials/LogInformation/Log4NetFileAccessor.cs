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
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.LogInformation
{
    [Export(typeof(IProvideLogMessages))]
    public class Log4NetFileAccessor : IProvideLogMessages
    {
        private const int MaximumNumberOfLines = 1000;

        public IEnumerable<LogMessage> GetMessages(params string[] emitters)
        {
            return GetMessages((IEnumerable<string>)emitters);
        }

        public IEnumerable<LogMessage> GetMessages(IEnumerable<string> emitters)
        {
            string[] emittersArray = emitters.ToArray();
            if (emittersArray.IsNullOrEmpty())
                return new LogMessage[0];
            string fileName = GetLog4NetFile();
            if (string.IsNullOrEmpty(fileName))
                return new LogMessage[0];
            IEnumerable<string> lines = GetLinesFromFile(fileName);
            IEnumerable<string> filteredLines = lines.Where(l => emittersArray.Any(l.Contains)).Reverse().Take(MaximumNumberOfLines).Reverse();
            return filteredLines.Select(l => new LogMessage(string.Empty, (LogMessage.CategoryEnum)(-1), l, null));
        }

        private IEnumerable<string> GetLinesFromFile(string filename)
        {
            try
            {
                return File.ReadAllLines(filename);
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private string GetLog4NetFile()
        {
            var fileAppender = GetLog4NetFileAppender();
            if (fileAppender == null)
                return string.Empty;
            return fileAppender.File;
        }

        private static FileAppender GetLog4NetFileAppender()
        {
            var hierarchy = LogManager.GetRepository() as Hierarchy;
            if (hierarchy == null)
                return null;
            FileAppender fileAppender = hierarchy.Root.Appenders.OfType<FileAppender>().FirstOrDefault();
            return fileAppender;
        }
    }
}
