using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace SharedLoaderNet.Tests
{
	public class WinApiTests
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate uint GetCurrentProcessId();

		private static uint GetProcessId()
		{
#if NETCOREAPP3_1 || NET48
			using (Process process = Process.GetCurrentProcess())
			{
				return (uint)process.Id;
			}
#else
			return (uint)Environment.ProcessId;
#endif
		}

		[Fact]
		public void GetCommandLineDelegate()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return;
			using (SharedLibrary sl = new SharedLibrary("Kernel32.dll"))
			{
				Assert.Equal(GetProcessId(), sl.GetDelegate<GetCurrentProcessId>(nameof(GetCurrentProcessId))());
			}
		}

#if NET5_0
		[Fact]
		public unsafe void GetCommandLinePointer()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return;
			using (SharedLibrary sl = new("Kernel32.dll"))
			{
				Assert.Equal(GetProcessId(), ((delegate* unmanaged<uint>)sl.GetPointer(nameof(GetCurrentProcessId)))());
			}
		}
#endif

		[Fact]
		public void FallbackLoad()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return;
			using (SharedLibrary sl = new SharedLibrary("a.test", "b.zip", "c.png", "e\0.jpg", "Kernel32.dll", "d.txt", ""))
			{
				Assert.Equal(GetProcessId(), sl.GetDelegate<GetCurrentProcessId>(nameof(GetCurrentProcessId))());
			}
		}
	}
}
