using System.Collections;

namespace NCrunch.Framework
{
    /// <summary>
    /// Sets a timeout value (in ms), after which NCrunch will immediately terminate and fail this test.
    /// </summary>
	public class TimeoutAttribute: System.Attribute
	{
		private IDictionary _properties;

		public TimeoutAttribute(int timeout)
		{
			_properties = new Hashtable();
			_properties["Timeout"] = timeout;
		}

		public IDictionary Properties
		{
			get { return _properties; }
		}
	}
}
