using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace SharedLoaderNet.Tests
{
	public class LibCTests
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		// ReSharper disable once InconsistentNaming
		private delegate int getpid();

		private static int GetProcessId()
		{
#if NETCOREAPP3_1 || NET48
			using (Process process = Process.GetCurrentProcess())
			{
				return process.Id;
			}
#else
			return Environment.ProcessId;
#endif
		}

		[Fact]
		public void GetCommandLineDelegate()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return;
			using (SharedLibrary sl = new SharedLibrary("libc.so"))
			{
				Assert.Equal(GetProcessId(), sl.GetDelegate<getpid>(nameof(getpid))());
			}
		}

#if NET5_0
		[Fact]
		public unsafe void GetCommandLinePointer()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return;
			using (SharedLibrary sl = new("libc.so"))
			{
				Assert.Equal(GetProcessId(), ((delegate* unmanaged<int>)sl.GetPointer(nameof(getpid)))());
			}
		}
#endif

		[Fact]
		public void FallbackLoad()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return;
			using (SharedLibrary sl = new SharedLibrary("a.test", "b.zip", "c.png", "e\0.jpg", "libc.so", "d.txt", ""))
			{
				Assert.Equal(GetProcessId(), sl.GetDelegate<getpid>(nameof(getpid))());
			}
		}
	}
}
