using System;

namespace SharedLoaderNet.Loaders
{
	internal interface ILibraryLoader
	{
		bool Supported { get; }
		Type InnerExceptionType { get; }
		IntPtr Load(string name);
		IntPtr GetSymbol(IntPtr module, string name);
		void Free(IntPtr module);
	}
}
