using SharedLoaderNet.Reflections;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SharedLoaderNet
{
	public abstract class DynamicLibrary : IDisposable
	{
		private readonly SharedLibrary library;

		public DynamicLibrary(string name, bool disposeable)
		{
			library = new SharedLibrary(name, disposeable);
			LoadBindings();
		}

#if NET5_0
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields |
									DynamicallyAccessedMemberTypes.PublicFields |
									DynamicallyAccessedMemberTypes.NonPublicProperties |
									DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
		private unsafe void LoadBindings()
		{
			Type type = GetType();
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				bool isDelegate = typeof(Delegate).IsAssignableFrom(field.FieldType);
				bool isFunctionPointer = typeof(IntPtr).IsAssignableFrom(field.FieldType);
				if (!isDelegate && !isFunctionPointer)
					continue;
				foreach (CustomAttributeData attribute in field.CustomAttributes)
				{
					if (attribute.AttributeType != typeof(UnmanagedMethodAttribute))
						continue;
					if (attribute.ConstructorArguments.Count >= 2 && !(bool)(type.GetField((string)attribute.ConstructorArguments[1].Value ?? throw new NullReferenceException(),
						BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
						)?.GetValue(this) ?? throw new NullReferenceException()))
						break;
					string entrypoint = (string)type.GetField((string)attribute.ConstructorArguments[0].Value ?? throw new NullReferenceException(),
						BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(this);
					if (entrypoint == null)
						throw new Exception($"You need to provide the entrypoint for {field.Name}");
					Debug.WriteLine(field.FieldType.FullName);
					if (isDelegate)
						field.SetValue(this, library.GetDelegate(entrypoint, field.FieldType));
					else
						field.SetValue(this, new IntPtr(library.GetPointer(entrypoint)));
					break;
				}
			}
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				bool isDelegate = typeof(Delegate).IsAssignableFrom(property.PropertyType);
				bool isFunctionPointer = typeof(IntPtr).IsAssignableFrom(property.PropertyType);
				if (!isDelegate && !isFunctionPointer || property.CanWrite == false)
					continue;
				foreach (CustomAttributeData attribute in property.CustomAttributes)
				{
					if (attribute.AttributeType != typeof(UnmanagedMethodAttribute))
						continue;
					if (attribute.ConstructorArguments.Count >= 2 && !(bool)(type.GetField((string)attribute.ConstructorArguments[1].Value ?? throw new NullReferenceException(),
						BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
						)?.GetValue(this) ?? throw new NullReferenceException()))
						break;
					string entrypoint = (string)type.GetField((string)attribute.ConstructorArguments[0].Value ?? throw new NullReferenceException(),
							BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(this);
					if (entrypoint == null)
						throw new Exception($"You need to provide the entrypoint for {property.Name}");
					if (isDelegate)
						property.SetValue(this, library.GetDelegate(entrypoint, property.PropertyType));
					else
						property.SetValue(this, new IntPtr(library.GetPointer(entrypoint)));
					break;
				}
			}
		}

		public void Dispose()
		{
			library.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
