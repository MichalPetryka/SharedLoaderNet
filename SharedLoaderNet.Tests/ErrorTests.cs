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
		[InlineData("")]
		public void ErrorLoadTest(string name)
		{
			Assert.Throws<DllNotFoundException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(name))
				{
				}
			});
		}

		[Theory]
		[InlineData("")]
		[InlineData("qwerty")]
		[InlineData("qwe\0rty")]
		public unsafe void ErrorSymbolTest(string name)
		{
			Assert.Throws<EntryPointNotFoundException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so"))
				{
					Assert.Equal(IntPtr.Zero, new IntPtr(sl.GetPointer(name)));
				}
			});
		}

		[Fact]
		public unsafe void NullSymbolTest()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				using (SharedLibrary sl = new SharedLibrary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : "libc.so"))
				{
					Assert.Equal(IntPtr.Zero, new IntPtr(sl.GetPointer(null)));
				}
			});
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
