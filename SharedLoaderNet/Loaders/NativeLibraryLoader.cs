using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharedLoaderNet.Loaders
{
	internal class NativeLibraryLoader : ILibraryLoader
	{
		public bool Supported =>
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET472
			false;
#else
			true;
#endif
		public Type InnerExceptionType => null;

		public IntPtr Load(string name)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET472
			throw new NotSupportedException();
#else
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			return NativeLibrary.Load(name);
#endif
		}

		public IntPtr GetSymbol(IntPtr module, string name)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET472
			throw new NotSupportedException();
#else
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			return NativeLibrary.GetExport(module, name);
#endif
		}

		public void Free(IntPtr module)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET472
			throw new NotSupportedException();
#else
			if (module == IntPtr.Zero)
				throw new ArgumentException("Module cannot be zero", nameof(module));
			NativeLibrary.Free(module);
#endif
		}
	}
}
