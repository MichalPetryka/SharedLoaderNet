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

		[Fact]
		public void NullPath()
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				Assert.Throws<ArgumentNullException>(() =>
				{
					Assert.Equal(IntPtr.Zero, loader.Load(null));
				});
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\0")]
		[InlineData(" \0 ")]
		public void EmptyOrWhitespacePath(string name)
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				Assert.Throws<ArgumentException>(() =>
				{
					Assert.Equal(IntPtr.Zero, loader.Load(name));
				});
			}
		}

		[Theory]
		[InlineData("qwerty")]
		[InlineData("qwe\0rty")]
		public void InvalidSymbol(string name)
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				IntPtr module = loader.Load(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : TestUtils.LibC);
				try
				{
					Assert.Equal(loader.InnerExceptionType, Assert.Throws<EntryPointNotFoundException>(() =>
					{
						Assert.Equal(IntPtr.Zero, loader.GetSymbol(module, name));
					}).InnerException?.GetType());
				}
				finally
				{
					loader.Free(module);
				}
			}
		}

		[Fact]
		public void NullSymbol()
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				IntPtr module = loader.Load(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : TestUtils.LibC);
				try
				{
					Assert.Throws<ArgumentNullException>(() =>
					{
						Assert.Equal(IntPtr.Zero, loader.GetSymbol(module, null));
					});
				}
				finally
				{
					loader.Free(module);
				}
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\0")]
		[InlineData(" \0 ")]
		public void EmptyOrWhitespaceSymbol(string name)
		{
			foreach (ILibraryLoader loader in Loaders)
			{
				IntPtr module = loader.Load(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : TestUtils.LibC);
				try
				{
					Assert.Throws<ArgumentException>(() =>
					{
						Assert.Equal(IntPtr.Zero, loader.GetSymbol(module, name));
					});
				}
				finally
				{
					loader.Free(module);
				}
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

		[Fact]
		public void NativeLibrary()
		{
			NativeLibraryLoader loader = new NativeLibraryLoader();
			if (!loader.Supported)
			{
				Assert.Throws<NotSupportedException>(() => { loader.Load(null); });
				Assert.Throws<NotSupportedException>(() => { loader.GetSymbol(IntPtr.Zero, null); });
				Assert.Throws<NotSupportedException>(() => { loader.Free(IntPtr.Zero); });
			}
		}
	}
}
