using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SharedLoaderNet.Tests
{
	public class ErrorTests
	{
		[Theory]
		[InlineData("a.test")]
		[InlineData("b.zip")]
		[InlineData("c.png")]
		[InlineData("d.txt")]
		[InlineData("e\0.jpg")]
		public void ErrorLoadTest(string name)
		{
			Assert.Throws<DllNotFoundException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(name))
				{
				}
			});
		}

		[Fact]
		public void NullLoadTest()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(null, true))
				{
				}
			});
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\0")]
		[InlineData(" \0 ")]
		public void EmptyOrWhitespaceLoadTest(string name)
		{
			Assert.Throws<ArgumentException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(name))
				{
				}
			});
		}

		[Theory]
		[InlineData("qwerty")]
		[InlineData("qwe\0rty")]
		public unsafe void ErrorSymbolTest(string name)
		{
			using (SharedLibrary sl = new SharedLibrary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so.6"))
			{
				Assert.Throws<EntryPointNotFoundException>(() =>
				{
					Assert.Equal(IntPtr.Zero, new IntPtr(sl.GetPointer(name)));
				});
			}
		}

		[Fact]
		public unsafe void NullSymbolTest()
		{
			using (SharedLibrary sl = new SharedLibrary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so.6"))
			{
				Assert.Throws<ArgumentNullException>(() =>
				{
					Assert.Equal(IntPtr.Zero, new IntPtr(sl.GetPointer(null)));
				});
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		[InlineData("\0")]
		[InlineData(" \0 ")]
		public unsafe void EmptyOrWhitespaceSymbolTest(string name)
		{
			using (SharedLibrary sl = new SharedLibrary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so.6"))
			{
				Assert.Throws<ArgumentException>(() =>
				{
					Assert.Equal(IntPtr.Zero, new IntPtr(sl.GetPointer(name)));
				});
			}
		}

		[Theory]
		[InlineData("a.test", "b.zip", "c.png", "d.txt", "", "e\0.jpg")]
		[InlineData("", "../../b.txt")]
		[InlineData("a.test")]
		public void ErrorFallbackTest(params string[] names)
		{
			Assert.Throws<AggregateException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(names))
				{
				}
			});
		}

		[Theory]
		[InlineData]
		[InlineData(null)]
		public void ErrorNoFallbacksTest(params string[] names)
		{
			Assert.Throws<ArgumentException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(names))
				{
				}
			});
		}
	}
}
