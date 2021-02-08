using System;
using SharedLoaderNet.Loaders;

namespace SharedLoaderNet.Tests
{
	internal static class TestUtils
	{
		private static string _libC;
		public static string LibC
		{
			get
			{
				if (_libC != null)
					return _libC;
				LibDlLoader loader = new LibDlLoader();
				foreach (string libc in new[] { "libc.so", "libc.so.6", "libc.so.5", "libc.so.4", "libc.so.3", "libc.so.2", "libc.so.1" })
				{
					try
					{
						loader.Free(loader.Load(libc));
						_libC = libc;
						return _libC;
					}
					catch (DllNotFoundException)
					{
						// ignore
					}
				}

				throw new PlatformNotSupportedException();
			}
		}
	}
}
