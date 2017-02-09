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

namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// The tag listener notifies when a tag value changes. It is an event based approach, independent of the underlying implementation (it could be polling or anything else).
    /// It implements a classic TagChanged event but provides also an RX observable to allow subscription to a Tag changed "stream".
    /// </summary>
    /// <remarks>
    /// Depending on the underlying implementation, the communication to PLC might work with polling. Therefore the listener can be
    /// started and stopped for performance reasons.
    /// </remarks>
    public interface ITagListener : IDisposable
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Occurs when a tag value changed. Event contains new tag value information in event args.
        /// </summary>
        event EventHandler<TagChangedEventArgs> TagChanged;

        /// <summary>
        /// Occurs on each polling cycle.
        /// </summary>
        event EventHandler PollingEvent;

        /// <summary>
        /// Occures on each polling cycle if one of the tags has changed
        /// The argument is a list of all changed tags
        /// </summary>
        event EventHandler<List<Tag>> CollectedTagChanged;

        /// <summary>
        /// Occurs when a PLC communication problem occured.
        /// Normally the event contains a PlcCommunicationException.
        /// </summary>
        /// <remarks>
        /// This event is a replacement for exception. Because all TagListener implementations might work asynchronous when StartListening (polling timers, asynch PLC events etc.) catching exceptions during read operation is not possible. 
        /// It will raise this event instead.
        /// </remarks>
        event EventHandler<Exception> CommunicationProblemOccured;

        /// <summary>
        /// Occurs when the PLC connection state changed (online/offline).
        /// </summary>
        event EventHandler<bool> ConnectionStateChanged;

        /// <summary>
        /// Determines whether this instance is connected and running.
        /// </summary>
        /// <remarks>
        /// Note for Beckhoff implementation: If PLC is in CONFIG mode it will return false. If PLC is in RUN mode but STOPPED it will return false as well.
        /// It will return true if PLC is in RUN mode and STARTED.
        /// </remarks>
        /// <returns>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </returns>
        bool IsConnected { get; }

        /// <summary>
        /// Initializes this instance to listen on the specified <paramref name="address"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="address">The address to PLC.</param>
        /// <param name="path">The path (in case of Beckhoff ads port).</param>
        void Initialize(string address, int path = 0);

        /// <summary>
        /// Gets the tag observable to allow subscribing to a stream of changed Tags.
        /// </summary>
        /// <remarks>
        /// Using ReactiveExtensions it is possible to combine multiple TagListeners and using smart Linq queries to filter out needed informations from PLC.
        /// This would also solve concurrency issues for TagListeners running on different threads and having different refresh rates etc.
        /// It is like a powerful alternative to classic .NET events.
        /// </remarks>
        /// <returns>
        /// Observable of Tag.
        /// </returns>
        IObservable<Tag> GetTagStream();

        /// <summary>
        /// Adds a new tag to observe.
        /// </summary>
        /// <remarks>
        /// This methods adds only a single tag, child tags will be ignored.
        /// </remarks>
        /// <see cref="AddTagsRecursively"/>
        /// <param name="tag">The tag.</param>
        void AddTag(Tag tag);

        /// <summary>
        /// Adds a tag list provided by a static class.
        /// </summary>
        /// <param name="tagContainer">Static class with Tag fields. Nested sub classes are possible.</param>
        void AddTags(Type tagContainer);

        /// <summary>
        /// Adds the tags recursively.
        /// </summary>
        /// <param name="rootTag">The root tag.</param>
        void AddTagsRecursively(Tag rootTag);

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <returns>Collection of tags.</returns>
        ICollection<Tag> GetTags();

        /// <summary>
        /// Adds a UDT (user defined type or struct) handler.
        /// </summary>
        /// <remarks>
        /// Supported for Beckhoff PLC systems.
        /// Rockwell PLC systems does not support this method. Use the other overloaded AddUdtHandler method instead and pass a converter function.
        /// </remarks>
        /// <typeparam name="TNetType">The type of the et type.</typeparam>
        /// <param name="plcDataType">Type of the PLC data.</param>
        void AddUdtHandler<TNetType>(string plcDataType);

        /// <summary>
        /// Adds a UDT (user defined type) handler.
        /// This extends the standard type convertion for BOOL, INT, etc. with custom types.
        /// </summary>
        /// <typeparam name="TCipStruct">
        /// The type of the CIP (Common Industrial Protocol) struct. 
        /// The struct members must have exactly ordering and type and match with the PLC UDT object.
        /// </typeparam>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="convertToCustomValue">
        /// Function delegate for converting the CipStruct to a custom object. The converted object is finally stored in the Tag value property.
        /// </param>
        void AddUdtHandler<TCipStruct>(string dataType, ConvertToCustomValueConverterFunction convertToCustomValue);

        /// <summary>
        /// Reads a tag and waits until the new value is received.
        /// In case reading is not possible the tag value will be null, and the Tag is still not initialized.
        /// The Tag.IsActive flag is not considered by this operation.
        /// </summary>
        /// <param name="tag">Tag to read the value for.</param>
        void ReadTagSynchronously(Tag tag);

        /// <summary>
        /// Refreshes all Tag values and raises TagChanged events for all Tags, independent from its IsActive flag.
        /// </summary>
        void RefreshAll();

        /// <summary>
        /// Gets or sets the refresh rate for polling.
        /// </summary>
        /// <value>
        /// The refresh rate in milliseconds.
        /// </value>
        /// <remarks>
        /// Common response time for a single abool is 15 - 20 ms.
        /// Be careful with to low polling rates, especially when accessing with multiple threads/processes to the PLC.
        /// </remarks>
        double RefreshRate { get; set; }

        /// <summary>
        /// Removes the specified <paramref name="tag"/> from observation.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        void RemoveTag(Tag tag);

        /// <summary>
        /// Starts the listener.
        /// </summary>
        void StartListening();

        /// <summary>
        /// Starts the listening.
        /// </summary>
        /// <param name="address">The address to PLC.</param>
        /// <param name="path">The path (in case of Beckhoff ads port).</param>
        void StartListening(string address, int path);

        /// <summary>
        /// Stops the listener.
        /// </summary>
        void StopListening();

        /// <summary>
        /// Gets the address and path with following format: 192.168.2.224:851
        /// </summary>
        /// <value>
        /// The address and path.
        /// </value>
        string AddressAndPath { get; set; }

        /// <summary>
        /// Gets the port which is the path for Rockwell and ADS-port for Beckhoff PLC systems.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        int Port { get; }
    }

    /// <summary>
    /// Converts a Logix Tag Value to a custom object.
    /// Not needed for Beckhoff implementation.
    /// </summary>
    /// <param name="cipStruct">The cip mapping struct.</param>
    /// <returns>
    /// Custom object which represents the value of the tag.
    /// </returns>
    public delegate object ConvertToCustomValueConverterFunction(object cipStruct);
}
