using System;

namespace NCrunch.Framework
{
    /// <summary>
    /// When NCrunch runs a test declaring this attribute, it will create a new process for it that is terminated at the end of its execution.
    /// The test does not share this process with any other test.
    /// </summary>
	public class IsolatedAttribute: Attribute
	{
	}
}
