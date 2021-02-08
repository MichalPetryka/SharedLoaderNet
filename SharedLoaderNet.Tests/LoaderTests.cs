using SharedLoaderNet.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace SharedLoaderNet.Tests
{
	public class LoaderTests
	{
		private static IEnumerable<ILibraryLoader> Loaders =>
			typeof(SharedLibrary).Assembly.GetTypes()
							.Where(type => type.IsClass && !type.IsAbstract &&
											typeof(ILibraryLoader).IsAssignableFrom(type))
							.Select(type => (ILibraryLoader)Activator.CreateInstance(type))
							.Where(loader => (loader ?? throw new NullReferenceException()).Supported);

		[Theory]
		[InlineData("")]
		[InlineData("a.txt")]
		[InlineData("b.zip")]
		[InlineData("c\0.png")]
		public void InvalidPath(string name)
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				Assert.Equal(loader.InnerExceptionType, Assert.Throws<DllNotFoundException>(() =>
				{
					Assert.Equal(IntPtr.Zero, loader.Load(name));
				}).InnerException?.GetType());
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData("qwerty")]
		[InlineData("qwe\0rty")]
		public void InvalidSymbol(string name)
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				Assert.Equal(loader.InnerExceptionType, Assert.Throws<EntryPointNotFoundException>(() =>
				{
					IntPtr module = loader.Load(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so");
					try
					{
						Assert.Equal(IntPtr.Zero, loader.GetSymbol(module, name));
					}
					finally
					{
						loader.Free(module);
					}
				}).InnerException?.GetType());
			}
		}

		[Fact]
		public void NullSymbol()
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				Assert.Throws<ArgumentNullException>(() =>
				{
					IntPtr module = loader.Load(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so");
					try
					{
						Assert.Equal(IntPtr.Zero, loader.GetSymbol(module, null));
					}
					finally
					{
						loader.Free(module);
					}
				});
			}
		}

		[Fact]
		public void ZeroModule()
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				Assert.Throws<ArgumentException>(() =>
				{
					loader.Free(IntPtr.Zero);
				});
			}
		}
	}
}
