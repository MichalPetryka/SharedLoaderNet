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
		private static extern IntPtr dlopen([MarshalAs(StringType)] string name, LibDlFlags flags);

		[DllImport(LibDl, EntryPoint = "dlsym", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr dlsym(IntPtr module, [MarshalAs(StringType)] string name);

		[DllImport(LibDl, EntryPoint = "dlclose", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
		private static extern int dlclose(IntPtr handle);

		[DllImport(LibDl, EntryPoint = "dlerror", CharSet = CharSet.Ansi, ExactSpelling = true, BestFitMapping = false, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(StringType)]
		private static extern string dlerror();

		[Flags]
		private enum LibDlFlags
		{
			Lazy = 0x00001,
			Now = 0x00002,
			Global = 0x00100,
			Local = 0,
			NoDelete = 0x01000,
			NoLoad = 0x00004,
			DeepBind = 0x00008
		}

		public bool Supported { get; } = TryLoad();
		public Type InnerExceptionType => typeof(LibDlException);

		private static bool TryLoad()
		{
			try
			{
				return dlopen(null, LibDlFlags.Local | LibDlFlags.Now) != IntPtr.Zero;
			}
			catch
			{
				return false;
			}
		}

		public IntPtr Load(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			IntPtr module = dlopen(name, LibDlFlags.Local | LibDlFlags.Now);
			if (module == IntPtr.Zero)
			{
				throw new DllNotFoundException("", new LibDlException(dlerror()));
			}
			return module;
		}

		public IntPtr GetSymbol(IntPtr module, string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			IntPtr symbol = dlsym(module, name);
			if (symbol == IntPtr.Zero)
			{
				throw new EntryPointNotFoundException("", new LibDlException(dlerror()));
			}
			return symbol;
		}

		public void Free(IntPtr module)
		{
			if (module == IntPtr.Zero)
				throw new ArgumentException("Module cannot be zero", nameof(module));
			if (dlclose(module) != 0)
			{
				throw new LibDlException(dlerror());
			}
		}
	}
}
