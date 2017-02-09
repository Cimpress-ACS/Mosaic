namespace NCrunch.Framework
{
    /// <summary>
    /// Declares as test as making inclusive use of a named resource that exists outside the test process.  Tests that are marked as making
    /// use of the same resource will not be executed together in parallel by NCrunch.
    /// </summary>
	public class InclusivelyUsesAttribute : ResourceUsageAttribute
	{
		public InclusivelyUsesAttribute(params string[] resourceNames): base(resourceNames) { }
        public InclusivelyUsesAttribute(string resourceName) : base(resourceName) { }
    }
}
