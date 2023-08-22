using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Unix.Native
{
	public sealed class CdeclFunction
	{
		private readonly string library;

		private readonly string method;

		private readonly Type returnType;

		private readonly AssemblyName assemblyName;

		private readonly AssemblyBuilder assemblyBuilder;

		private readonly ModuleBuilder moduleBuilder;

		private Hashtable overloads;

		public CdeclFunction(string library, string method)
			: this(library, method, typeof(void))
		{
		}

		public CdeclFunction(string library, string method, Type returnType)
		{
			this.library = library;
			this.method = method;
			this.returnType = returnType;
			overloads = new Hashtable();
			assemblyName = new AssemblyName();
			assemblyName.Name = "Mono.Posix.Imports." + library;
			assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
		}

		public object Invoke(object[] parameters)
		{
			Type[] parameterTypes = GetParameterTypes(parameters);
			return CreateMethod(parameterTypes).Invoke(null, parameters);
		}

		private MethodInfo CreateMethod(Type[] parameterTypes)
		{
			string typeName = GetTypeName(parameterTypes);
			lock (overloads)
			{
				MethodInfo methodInfo = (MethodInfo)overloads[typeName];
				if (methodInfo != null)
				{
					return methodInfo;
				}
				TypeBuilder typeBuilder = CreateType(typeName);
				typeBuilder.DefinePInvokeMethod(method, library, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl, CallingConventions.Standard, returnType, parameterTypes, CallingConvention.Cdecl, CharSet.Ansi);
				methodInfo = typeBuilder.CreateType().GetMethod(method);
				overloads.Add(typeName, methodInfo);
				return methodInfo;
			}
		}

		private TypeBuilder CreateType(string typeName)
		{
			return moduleBuilder.DefineType(typeName, TypeAttributes.Public);
		}

		private static Type GetMarshalType(Type t)
		{
			switch (Type.GetTypeCode(t))
			{
			case TypeCode.Boolean:
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
				return typeof(int);
			case TypeCode.Byte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
				return typeof(uint);
			case TypeCode.Int64:
				return typeof(long);
			case TypeCode.UInt64:
				return typeof(ulong);
			case TypeCode.Single:
			case TypeCode.Double:
				return typeof(double);
			default:
				return t;
			}
		}

		private string GetTypeName(Type[] parameterTypes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[").Append(library).Append("] ")
				.Append(method);
			stringBuilder.Append("(");
			if (parameterTypes.Length != 0)
			{
				stringBuilder.Append(parameterTypes[0]);
			}
			for (int i = 1; i < parameterTypes.Length; i++)
			{
				stringBuilder.Append(",").Append(parameterTypes[i]);
			}
			stringBuilder.Append(") : ").Append(returnType.FullName);
			return stringBuilder.ToString();
		}

		private static Type[] GetParameterTypes(object[] parameters)
		{
			Type[] array = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = GetMarshalType(parameters[i].GetType());
			}
			return array;
		}
	}
}
