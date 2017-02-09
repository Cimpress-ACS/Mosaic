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


using NUnit.Framework;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class DataChannelTests
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        // TODO: introduce new GenericDataChannel to PLC sample project or use new method injection Beckhoff feature for new DataChannel approach


        /*
// IDataChannelListener
IDataChannelListener<DataChannelTestData> dataChannelListener = new DataChannelListener<DataChannelTestData>(tagListener, tagController);
dataChannelListener.SetChannel(new Tag("fbMOD_2.SIf.DtChnToLine", "MiddlePRG_1", "T_Ctrl_SIf_MOD_DtChnToPLC"));

dataChannelListener.DataReceived += DataChannelListenerDataReceived;
dataChannelListener.CommunicationProblemOccured += DataChannelListenerCommunicationProblemOccured;

// IDataChannelWriter
IDataChannelWriter dataChannelWriter = new DataChannelWriter(tagListener, tagController);
dataChannelWriter.CommunicationProblemOccured += DataChannelWriterCommunicationProblemOccured;

var dataChannelTag = new Tag("fbMOD_2.SIf.DtChnToPLC", "MiddlePRG_1", "T_Ctrl_SIf_MOD_DtChnToPLC");

dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 1 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 2 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 3 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 4 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 5 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 6 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 7 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 8 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 9 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 10 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 11 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 12 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 13 });
dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new DataChannelTestData { Test = 14 });

// this is optional
//dataChannelWriter.WaitWriteComplete();
Console.WriteLine("wrote 14 values over DataChannelManager");

while (true)
{
    Thread.Sleep(1000);
    Console.WriteLine("IsConnected = " + tagController.IsConnected);
}
 * 
 * */
    }
}
