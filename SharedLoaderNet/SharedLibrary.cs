using SharedLoaderNet.Loaders;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SharedLoaderNet
{
	public sealed class SharedLibrary : IDisposable
#if !NETSTANDARD2_0 && !NET472
										, IAsyncDisposable
#endif
	{
		internal static readonly ILibraryLoader Loader;

		private readonly object _freeLock = new object();
		internal readonly IntPtr _module;
		internal bool _disposeable;

		static SharedLibrary()
		{
			WindowsLoader windowsLoader = new WindowsLoader();
			if (windowsLoader.Supported)
			{
				Loader = windowsLoader;
				return;
			}
			LibDlLoader libDlLoader = new LibDlLoader();
			if (libDlLoader.Supported)
			{
				Loader = libDlLoader;
				return;
			}
			NativeLibraryLoader nativeLibraryLoader = new NativeLibraryLoader();
			if (nativeLibraryLoader.Supported)
			{
				Loader = nativeLibraryLoader;
				return;
			}

			throw new PlatformNotSupportedException();
		}

		public SharedLibrary(string name, bool disposeable = true)
		{
			if (Loader == null || !Loader.Supported)
			{
				throw new PlatformNotSupportedException();
			}

			_module = Loader.Load(name);
			_disposeable = disposeable;
		}

		public SharedLibrary(params string[] names) : this(true, names) { }

		public SharedLibrary(bool disposeable, params string[] names)
		{
			if (Loader == null || !Loader.Supported)
			{
				throw new PlatformNotSupportedException();
			}

			if (names == null || names.Length == 0)
				throw new ArgumentException("Library names must conatain at least one element", nameof(names));

			List<Exception> exceptions = new List<Exception>();
			foreach (string name in names)
			{
				try
				{
					_module = Loader.Load(name);
					break;
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
					_module = IntPtr.Zero;
				}
			}

			if (_module == IntPtr.Zero)
				throw new AggregateException(exceptions);

			_disposeable = disposeable;
		}

		public unsafe void* GetPointer(string name)
		{
			return Loader.GetSymbol(_module, name).ToPointer();
		}

		public T GetDelegate<T>(string name) where T : Delegate
		{
			return Marshal.GetDelegateForFunctionPointer<T>(Loader.GetSymbol(_module, name));
		}

		private void Free()
		{
			lock (_freeLock)
			{
				if (_disposeable)
					Loader.Free(_module);
				_disposeable = false;
			}
		}

		public void Dispose()
		{
			Free();
			GC.SuppressFinalize(this);
		}

#if !NETSTANDARD2_0 && !NET472
		public ValueTask DisposeAsync()
		{
			Dispose();
#if NETSTANDARD2_1 || NETCOREAPP3_1
			return default;
#else
			return ValueTask.CompletedTask;
#endif
		}
#endif

		~SharedLibrary()
		{
			Free();
		}
	}
}
