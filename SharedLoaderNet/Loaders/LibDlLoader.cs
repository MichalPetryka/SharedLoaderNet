using System;
using System.Runtime.InteropServices;

namespace SharedLoaderNet.Loaders
{
	internal sealed class LibDlLoader : ILibraryLoader
	{
		private const string LibDl = "libdl";
		private const UnmanagedType StringType =
#if NETSTANDARD2_0
			UnmanagedType.LPStr;
#else
			UnmanagedType.LPUTF8Str;
#endif

		[DllImport(LibDl, EntryPoint = "dlopen", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DlOpen([MarshalAs(StringType)] string name, LibDlFlags flags);

		[DllImport(LibDl, EntryPoint = "dlsym", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DlSym(IntPtr module, [MarshalAs(StringType)] string name);

		[DllImport(LibDl, EntryPoint = "dlclose", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		private static extern int DlClose(IntPtr handle);

		[DllImport(LibDl, EntryPoint = "dlerror", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr DlError();

		[Flags]
		private enum LibDlFlags
		{
			// ReSharper disable UnusedMember.Local
			Lazy = 0x00001,
			Now = 0x00002,
			Global = 0x00100,
			Local = 0,
			NoDelete = 0x01000,
			NoLoad = 0x00004,
			DeepBind = 0x00008
			// ReSharper restore UnusedMember.Local
		}

		public bool Supported { get; } = TryLoad();
		public Type InnerExceptionType => typeof(LibDlException);

		private static bool TryLoad()
		{
			try
			{
				return DlOpen(null, LibDlFlags.Local | LibDlFlags.Now) != IntPtr.Zero;
			}
			catch
			{
				return false;
			}
		}

		private static string GetError()
		{
#if NETSTANDARD2_0 || NET472
			return Marshal.PtrToStringAnsi(DlError());
#else
			return Marshal.PtrToStringUTF8(DlError());
#endif
		}

		public IntPtr Load(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name) || name.Trim() == "\0")
				throw new ArgumentException("Empty or whitespace module names are not allowed", nameof(name));
			IntPtr module = DlOpen(name, LibDlFlags.Local | LibDlFlags.Now);
			if (module == IntPtr.Zero)
			{
				throw new DllNotFoundException("", new LibDlException(GetError()));
			}
			return module;
		}

		public IntPtr GetSymbol(IntPtr module, string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name) || name.Trim() == "\0")
				throw new ArgumentException("Empty or whitespace symbol names are not allowed", nameof(name));
			IntPtr symbol = DlSym(module, name);
			if (symbol == IntPtr.Zero)
			{
				throw new EntryPointNotFoundException("", new LibDlException(GetError()));
			}
			return symbol;
		}

		public void Free(IntPtr module)
		{
			if (module == IntPtr.Zero)
				throw new ArgumentException("Module cannot be zero", nameof(module));
			if (DlClose(module) != 0)
			{
				throw new LibDlException(GetError());
			}
		}
	}
}
