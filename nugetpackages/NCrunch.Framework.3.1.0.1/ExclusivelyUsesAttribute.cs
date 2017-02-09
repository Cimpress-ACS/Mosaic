namespace NCrunch.Framework
{
    /// <summary>
    /// Declares as test as making exclusive use of a named resource that exists outside the test process.  Tests that are marked as making
    /// use of the same resource will not be executed together in parallel by NCrunch.
    /// </summary>
	public class ExclusivelyUsesAttribute: ResourceUsageAttribute
	{
		public ExclusivelyUsesAttribute(params string[] resourceName) : base(resourceName) {}
        public ExclusivelyUsesAttribute(string resourceName) : base(resourceName) { }
    }
}
