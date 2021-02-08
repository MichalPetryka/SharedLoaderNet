using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SharedLoaderNet.Loaders
{
	internal sealed class WindowsLoader : ILibraryLoader
	{
		private const string Kernel32 = "Kernel32";

		[DllImport(Kernel32, EntryPoint = "LoadLibraryW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport(Kernel32, EntryPoint = "GetProcAddress", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr GetProcAddress(IntPtr module, [MarshalAs(UnmanagedType.LPStr)] string name);

		[DllImport(Kernel32, EntryPoint = "FreeLibrary", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
		private static extern bool FreeLibrary(IntPtr module);

		public bool Supported { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		public Type InnerExceptionType => typeof(Win32Exception);

		public IntPtr Load(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name) || name.Trim() == "\0")
				throw new ArgumentException("Empty or whitespace module names are not allowed", nameof(name));
			IntPtr module = LoadLibrary(name);
			if (module == IntPtr.Zero)
			{
				throw new DllNotFoundException("", new Win32Exception());
			}

			return module;
		}

		public IntPtr GetSymbol(IntPtr module, string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name) || name.Trim() == "\0")
				throw new ArgumentException("Empty or whitespace symbol names are not allowed", nameof(name));
			IntPtr symbol = GetProcAddress(module, name);
			if (symbol == IntPtr.Zero)
			{
				throw new EntryPointNotFoundException("", new Win32Exception());
			}

			return symbol;
		}

		public void Free(IntPtr module)
		{
			if (module == IntPtr.Zero)
				throw new ArgumentException("Module cannot be zero", nameof(module));
			if (!FreeLibrary(module))
			{
				throw new Win32Exception();
			}
		}
	}
}
