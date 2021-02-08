using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharedLoaderNet.Reflections;
using Xunit;

namespace SharedLoaderNet.Tests
{
	public class DynamicLibraryTests
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		public delegate int GetCurrentProcessId();

		private static int ProcessId
		{
			get
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
		}

		public unsafe class TestClass : DynamicLibrary
		{
			private readonly string _getCurrentProcessIdEntrypoint = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? nameof(GetCurrentProcessId) : "getpid";

			private readonly string _getppidEntrypoint = "getppid";
			private readonly bool _isgetppidAvailable = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

			[UnmanagedMethod(nameof(_getCurrentProcessIdEntrypoint))]
			public readonly GetCurrentProcessId GetProcessId;

			[UnmanagedMethod(nameof(_getppidEntrypoint), nameof(_isgetppidAvailable))]
			public readonly GetCurrentProcessId GetParentProcessId;

#if NET5_0
			[UnmanagedMethod(nameof(_getCurrentProcessIdEntrypoint))]
			public readonly delegate* unmanaged<int> GetProcessIdPtr;
#endif

			public TestClass() : base(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Kernel32.dll" : TestUtils.LibC, true) { }
		}

		[Fact]
		public void GetCommandLineDelegate()
		{
			using (TestClass tc = new TestClass())
			{
				Assert.Equal(ProcessId, tc.GetProcessId());
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					Assert.Null(tc.GetParentProcessId);
				else
					Assert.NotNull(tc.GetParentProcessId);
			}
		}

#if NET5_0
		[Fact]
		public unsafe void GetCommandLinePointer()
		{
			using (TestClass tc = new())
			{
				Assert.Equal(ProcessId, tc.GetProcessIdPtr());
			}
		}
#endif
	}
}
