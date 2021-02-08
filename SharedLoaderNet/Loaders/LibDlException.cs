using System;

namespace SharedLoaderNet.Loaders
{
	public class LibDlException : Exception
	{
		internal LibDlException(string message) : base(message) { }
	}
}
