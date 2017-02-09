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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// A tag is like a variable in the PLC and hold the name, value, datatype etc.
    /// This is just a C# data container for it.
    /// </summary>
    [DebuggerDisplay("{Scope}.{Name} ({DataType})  Port:{AdsPort}  Area:{Area}")]
    public class Tag : IEquatable<Tag>, IComparable<Tag>
    {
        private readonly object _lock = new object();
        private readonly object _indexLock = new object();
        private long _indexGroup;
        private long _indexOffset;

        private readonly Subject<object> _subject = new Subject<object>();
        private int _bitSize;
        private string _dataType;
        private object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        {
            Type = TagType.Tag;
            Area = string.Empty;
            Childs = new List<Tag>();
            IsActive = true;
            MetaData = new TagMetaData
            {
                Comment = string.Empty,
                UnitForUser = string.Empty
            };

            ValueHasChanged = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag" /> class of DataType BOOL.
        /// </summary>
        /// <param name="name">The name of tag.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="adsPort">Optional ads port (Beckhoff only).</param>
        public Tag(string name, string scope, int adsPort = 0, string area = "")
            :this(name,scope,IEC61131_3_DataTypes.Boolean, adsPort, area)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="adsPort">Optional ads port (Beckhoff only).</param>
        public Tag(string name, string scope, string dataType, int adsPort = 0, string area = "")
        {
            Type = TagType.Tag;
            Name = name;
            Scope = scope;
            DataType = dataType;
            AdsPort = adsPort;
            Area = area;
            Childs = new List<Tag>();
            IsActive = true;
            MetaData = new TagMetaData
            {
                Comment = string.Empty,
                UnitForUser = string.Empty
            };

            ValueHasChanged = false;
        }

        public delegate void TagEventHandler(Tag sender, TagValueChangedEventArgs e);

        /// <summary>
        /// Occurs when value changed.
        /// </summary>
        public event TagEventHandler ValueChanged;

        /// <summary>
        /// This can be used to allow adding the same tag to a taglistener
        /// Specially needed for the generic page additions 
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Rx stream for value changed.
        /// Powerful alternative to standard .NET events.
        /// </summary>
        public object ValueStream
        {
            get { return _subject; }
        }

        /// <summary>
        /// Gets the meta data of the Tag.
        /// </summary>
        public TagMetaData MetaData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Tag is active.
        /// </summary>
        /// <remarks>
        /// This improves performance, deactivated Tags will not be read by TagListener.
        /// </remarks>
        /// <value>
        ///   <c>true</c> if [is active]; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets a value indicating whether this Tag is initialized which means a value was read at least one time.
        /// </summary>
        public bool IsInitialized
        {
            get { return _value != null; }
        }

        /// <summary>
        /// Gets the tag childs in case of hierarchical tag data structure.
        /// </summary>
        /// <value>
        /// The childs.
        /// If tag is a standard DataType the collections count is zero.
        /// </value>
        public ICollection<Tag> Childs { get; private set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public Tag Parent { get; set; }

        /// <summary>
        /// Locks the value to synchronize concurrent threads.
        /// </summary>
        public void LockValue()
        {
            Monitor.Enter(_lock);
        }

        /// <summary>
        /// Releases the value to synchronize concurrent threads.
        /// </summary>
        public void ReleaseValue()
        {
            Monitor.Exit(_lock);
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public TagType Type { get; set; }

        /// <summary>
        /// Like a namespace for tags. The tags adsPort plus scope plus name is unique, but the tags name only is not.
        /// </summary>
        /// <remarks>
        /// In case of Beckhoff PLC system the Scope is the name of the Program or FunctionBlock. Each Program or FunctionBlock holds its own variable list.
        /// </remarks>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the ads port for Beckhoff. Each PLC Program has its own ads port in Beckhoff.
        /// </summary>
        public int AdsPort { get; set; }

        /// <summary>
        /// Gets or sets the name of tag.
        /// When the tag is located in a nested structure Name returns the whole path (with dot separators).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the tag.
        /// When the tag is located in a nested structure only the Name of variable will be returned.
        /// </summary>
        /// <value>
        /// The name of the nested.
        /// </value>
        public string NestedName
        {
            get { return Name.Split('.').LastOrDefault(); }
        }

        public string Description { get; set; }

        /// <summary>
        /// Specifies the PLC data type of this tag. For all supported data types look at <see cref="VP.FF.PT.Common.PlcCommunication.Infrastructure.IEC61131_3_DataTypes"/>.
        /// </summary>
        /// <remarks>
        /// For Beckhoff implementations Arrays are supported, e.g. ARRAY (0..9) OF INT.
        /// Also STRING DataType with length specifier is supported, e.g. STRING(31). If no length specifier is given it will use default string length of 80.
        /// Note: The string length does not include the null termination symbol.
        /// </remarks>
        public string DataType
        {
            get { return _dataType; }
            set
            {
                if (_dataType != value)
                {
                    if (StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.IsMatch(value) && ArrayCreator == null)
                    {
                        ArrayCreator = () => new ArrayList();
                    }
                    else
                    {
                        ArrayCreator = null;
                    }

                    _dataType = value;

                    // force
                    if (BitSize == 0)
                        DetectBitSize();
                }
            }
        }

        /// <summary>
        /// Beckhoff specific information.
        /// </summary>
        public long IndexGroup
        {
            get
            {
                lock(_indexLock)
                {
                    return _indexGroup;
                }
            }
            set
            {
                lock (_indexLock)
                {
                    _indexGroup = value;
                }
            }
        }

        /// <summary>
        /// Beckhoff specific information.
        /// </summary>
        public long IndexOffset
        {
            get
            {
                lock (_indexLock)
                {
                    return _indexOffset;
                }
            }
            set
            {
                lock(_indexLock)
                {
                    _indexOffset = value;
                }
            }
        }

        /// <summary>
        /// Attached Tag meta information.
        /// </summary>
        /// <remarks>
        /// In case of Beckhoff this property contains the attached comment of a PLC variable. For multi line comments use the (*...*) syntax because double slash // supports only one line.
        /// In case of Rockwell this property contains the Rockwell specific "Specifier" information.
        /// </remarks>
        public string Specifier { get; set; }

        /// <summary>
        /// Rockwell specific information.
        /// </summary>
        public string Attributes { get; set; }

        /// <summary>
        /// Gets or sets the value. This could also be a UDT (user defined type).
        /// Will raise a value changed event.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value 
        {
            get
            {
                return _value;
            }

            set
            {
                // don't raise event if value is still same or null
                if (_value != null && _value.Equals(value) || _value == null && value == null)
                {
                    ValueHasChanged = false;
                    return;
                }

                _value = value;
                ValueHasChanged = true;
                RaiseValueChanged(value);
            }
        }

        /// <summary>
        /// True if the Value has change/False if the value has not changed
        /// </summary>
        public bool ValueHasChanged { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can create a array. Created array instances of this delegate will be used
        /// as the value of this tag if the datatype of this tag is an array.
        /// </summary>
        public Func<IList> ArrayCreator { get; set; }

        /// <summary>
        /// Gets or sets a function that can determine whether the array should continue to read additional values.
        /// Reason for this is pure performance since reading an array creates a variable handle for each instance.
        /// Assuming we have a stop condition, the PLC reader wouldn't need to read the remaining, likely empty or
        /// unchanged values.
        /// </summary>
        public Func<object, bool> StopReadingArrayPositions { get; set; }

        /// <summary>
        /// Gets this tag with all its descendants in a flat enumerable.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Tag"/> instances.</returns>
        public IEnumerable<Tag> GetThisWithDescendantsFlat()
        {
            yield return this;
            foreach (Tag child in Childs)
                foreach (Tag descendant in child.GetThisWithDescendantsFlat())
                    yield return descendant;
        }

        /// <summary>
        /// Gets the path with the specified <paramref name="childPath"/>.
        /// Moves down the tree structure and returns the desired <see cref="Tag"/> instance.
        /// </summary>
        /// <param name="childPath">The path where the child can be found.</param>
        /// <returns>A <see cref="Tag"/> instance or null.</returns>
        public Tag GetChildTagUnderPath(string childPath)
        {
            if (childPath == null)
                return null;
            if (childPath.Length == 0)
                return this;
            string[] pathSegments = childPath.Split('.');
            Tag nextChild = Childs.FirstOrDefault(c => string.Equals(c.NestedName, pathSegments.First()));
            if (nextChild == null)
                return null;
            return nextChild.GetChildTagUnderPath(string.Join(".", pathSegments.Tail()));
        }

        public void DetectDataTypeForValue(object value)
        {
            if (DataType == null && 
                IEC61131_3_DataTypes.NetDataTypes.Values.Contains(value.GetType()))
            {
                DataType = IEC61131_3_DataTypes.NetDataTypes.FindKeyByValue(value.GetType());
            }
        }

        /// <summary>
        /// Gets the value casted in the specified <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The expected type the value should have.</typeparam>
        /// <exception cref="InvalidCastException">
        /// This operation throws and invalid operation exception if the value
        /// does not have the expected type.
        /// </exception>
        /// <returns>The value.</returns>
        public virtual T GetValue<T>()
        {
            try
            {
                return (T) Value;
            }
            catch (InvalidCastException exception)
            {
                throw new InvalidCastException(new StringBuilder()
                    .AppendFormat("Tried to cast the value '{0}' to the type '{1}'.", Value, typeof(T)).AppendLine()
                      .AppendLine("Unfortunately this cast is not valid. Please check your code.").ToString()
                      ,exception);
            }
        }

        /// <summary>
        /// Returns a value from an array with proper C# type.
        /// </summary>
        /// <typeparam name="T">Type for casting. Must match to the PLC type, e.g. int is DINT, float is REAL and short is BYTE.</typeparam>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        /// <exception cref="VP.FF.PT.Common.PlcCommunication.TagException">Expected an object array for Tag.Value but cast is not possible!</exception>
        /// <exception cref="System.IndexOutOfRangeException">Index exceeds array boundaries.</exception>
        public T ArrayValue<T>(int index)
        {
            var retArray = Value as object[];

            if (retArray == null)
                throw new TagException("Expected an object array for Tag.Value but cast is not possible!", this);

            if (index >= retArray.Length)
                throw new IndexOutOfRangeException("index " + index + " exceeds array length of " + retArray.Length);

            return (T)retArray[index];
        }

        /// <summary>
        /// Returns an array with proper C# type.
        /// </summary>
        /// <typeparam name="T">Type for casting. Must match to the PLC type, e.g. int is DINT, float is REAL and short is BYTE.</typeparam>
        /// <returns>The array of type T</returns>
        /// <exception cref="VP.FF.PT.Common.PlcCommunication.TagException">Expected an object array for Tag.Value but cast is not possible!</exception>
        public T[] ArrayValues<T>()
        {
            var enumerable = Value as IEnumerable;
            if (enumerable == null)
                throw new TagException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to read an array value of type '{0}' from tag '{1}'.", typeof(T), this).AppendLine()
                    .AppendFormat("This tag has the value '{0}' which is cannot get casted into the desired array.", Value).AppendLine().ToString(), this);

            return enumerable.Cast<T>().ToArray();
        }

        public void ClearValue()
        {
            _value = null;
        }

        /// <summary>
        /// Gets or sets the number of bits are needed to store the DataType value.
        /// </summary>
        /// <value>
        /// The size of DataType.
        /// </value>
        public int BitSize
        {
            get
            {
                if (_bitSize == 0)
                    DetectBitSize();

                return _bitSize;
            }

            set { _bitSize = value; }
        }

        public static bool operator ==(Tag left, Tag right)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((object)left == null || (object)right == null)
                return false;

            // scope and name is unique and both in combination can be used as a primary key
            if (left.Scope == right.Scope && left.Name == right.Name && left.AdsPort == right.AdsPort && left.Area == right.Area)
                return true;

            return false;
        }

        public static bool operator !=(Tag left, Tag right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public virtual bool Equals(Tag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Scope, other.Scope) && string.Equals(Name, other.Name)  && string.Equals(Area, other.Area) && AdsPort == other.AdsPort;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Tag) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Scope != null ? Scope.GetHashCode() : 0)*397) ^ (Name != null ? Name.GetHashCode() : 0) + (AdsPort);
            }
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. 
        /// The return value has the following meanings: Value Meaning Less than zero This object is less than the other parameter.
        /// Zero This object is equal to other. Greater than zero This object is greater than other.
        /// </returns>
        public int CompareTo(Tag other)
        {
            return String.CompareOrdinal(AdsPort.ToString() + Area + Scope + Name, other.AdsPort.ToString() + other.Area + other.Scope + other.Name);
        }

        private void DetectBitSize()
        {
            int bitSize;
            if (IEC61131_3_DataTypes.BitSizes.TryGetValue(DataType, out bitSize))
            {
                _bitSize = bitSize;
            }

            if (DataType == IEC61131_3_DataTypes.String && Value != null)
            {
                if (Value != null)
                    _bitSize = (((string)Value).Length + 1) * 8;
                else
                    _bitSize = (80 + 1) * 8;
            }

            if (StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.IsMatch(DataType))
            {
                _bitSize = ParseBitSizeForArray();
            }

            if (StructuredTextSyntaxRegexHelper.StringDataTypeRegex.IsMatch(DataType))
            {
                var groups = StructuredTextSyntaxRegexHelper.StringDataTypeRegex.Matches(DataType)[0].Groups;

                if (groups.Count == 2 && groups[1].Value.Length > 0)
                {
                    _bitSize = (Int32.Parse(groups[1].Value) + 1) * 8;
                }
                else
                {
                    _bitSize = 81 * 8;
                }
            }
        }

        private int ParseBitSizeForArray()
        {
            Match match = StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.Match(DataType);

            if (match.Groups.Count != 4)
                throw new TagException("Tag " + Scope + "." + Name + " has invalid DataType " + DataType, this);

            int lowerBound = Convert.ToInt32(match.Groups[1].Value);
            int upperBound = Convert.ToInt32(match.Groups[2].Value);
            string dataType = match.Groups[3].Value;

            int bitSize;

            if (!IEC61131_3_DataTypes.BitSizes.TryGetValue(dataType, out bitSize))
                return -1;

            if (upperBound <= lowerBound)
                throw new TagException("Tag " + Scope + "." + Name + " has invalid ARRAY boundaries: " + DataType, this);

            int size = upperBound - lowerBound + 1;

            return size * IEC61131_3_DataTypes.BitSizes[dataType];
        }

        private void RaiseValueChanged(object value)
        {
            _subject.OnNext(value);

            if (ValueChanged != null)
                ValueChanged(this, new TagValueChangedEventArgs(value));
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return GetType().Name + ": " + Name;
        }
    }
}
