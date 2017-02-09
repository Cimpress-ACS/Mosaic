using System;

namespace NCrunch.Framework
{
    /// <summary>
    /// Tests implementing this attribute will not be run in parallel with any other test when they are executed by NCrunch.
    /// </summary>
	public class SerialAttribute: Attribute
	{
	}
}
