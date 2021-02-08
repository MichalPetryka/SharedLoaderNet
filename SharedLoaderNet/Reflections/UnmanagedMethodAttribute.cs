using System;

namespace SharedLoaderNet.Reflections
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class UnmanagedMethodAttribute : Attribute
	{
		public UnmanagedMethodAttribute(string entrypointData)
		{
		}

		public UnmanagedMethodAttribute(string entrypointData, string availabilityData)
		{
		}
	}
}
