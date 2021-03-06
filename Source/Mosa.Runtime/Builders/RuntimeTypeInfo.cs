﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Runtime;
using System.Collections.Generic;
using System.Reflection;

namespace System
{
	public sealed unsafe class RuntimeTypeInfo : TypeInfo
	{
		private MDTypeDefinition* typeDefinition;
		private Assembly assembly;

		//private RuntimeTypeHandle handle;
		private string assemblyQualifiedName;

		private string name;
		private string @namespace;
		private string fullname;
		private TypeCode typeCode;
		private TypeAttributes attributes;
		private Type baseType;
		private Type declaringType;
		private Type elementType;
		private Type asType;
		private LinkedList<CustomAttributeData> customAttributesData = null;

		internal readonly Type ValueType = typeof(ValueType);
		internal readonly Type EnumType = typeof(Enum);

		public override string AssemblyQualifiedName
		{
			get { return assemblyQualifiedName; }
		}

		public override Assembly Assembly
		{
			get { return assembly; }
		}

		public override TypeAttributes Attributes
		{
			get { return attributes; }
		}

		public override Type BaseType
		{
			get { return (IsInterface) ? null : baseType; }
		}

		public override bool ContainsGenericParameters
		{
			get { throw new NotImplementedException(); }
		}

		public override IEnumerable<CustomAttributeData> CustomAttributes
		{
			get
			{
				if (customAttributesData == null)
				{
					// Custom Attributes Data - Lazy load
					customAttributesData = new LinkedList<CustomAttributeData>();
					if (typeDefinition->CustomAttributes != null)
					{
						var customAttributesTable = typeDefinition->CustomAttributes;
						var customAttributesCount = customAttributesTable->NumberOfAttributes;
						for (uint i = 0; i < customAttributesCount; i++)
						{
							RuntimeCustomAttributeData cad = new RuntimeCustomAttributeData(customAttributesTable->GetCustomAttribute(i));
							customAttributesData.AddLast(cad);
						}
					}
				}

				return customAttributesData;
			}
		}

		public override MethodBase DeclaringMethod
		{
			get { throw new NotImplementedException(); }
		}

		public override Type DeclaringType
		{
			get { return declaringType; }
		}

		public override string FullName
		{
			get { return fullname; }
		}

		public override int GenericParameterPosition
		{
			get { throw new NotSupportedException(); }
		}

		public override Type[] GenericTypeArguments
		{
			get { return new Type[0]; }
		}

		public override bool IsEnum
		{
			get { return BaseType == EnumType; }
		}

		public override bool IsGenericParameter
		{
			// We don't know so just return false
			get { return false; }
		}

		public override bool IsGenericType
		{
			// We don't know so just return false
			get { return false; }
		}

		public override bool IsGenericTypeDefinition
		{
			// We don't know so just return false
			get { return false; }
		}

		public override bool IsSerializable
		{
			// We don't know so just return false
			get { return false; }
		}

		public override string Name
		{
			get { return name; }
		}

		public override string Namespace
		{
			get { return @namespace; }
		}

		public RuntimeTypeInfo(RuntimeType type, Assembly assembly)
		{
			var handle = type.TypeHandle;
			asType = type;
			this.assembly = assembly;

			//this.handle = handle;
			typeDefinition = (MDTypeDefinition*)((uint**)&handle)[0];

			assemblyQualifiedName = typeDefinition->Name;   // TODO
			name = typeDefinition->Name;                    // TODO
			@namespace = typeDefinition->Name;              // TODO
			fullname = typeDefinition->Name;

			typeCode = typeDefinition->TypeCode;
			attributes = typeDefinition->Attributes;

			// Base Type
			if (typeDefinition->ParentType != null)
			{
				RuntimeTypeHandle parentHandle = new RuntimeTypeHandle();
				((uint**)&parentHandle)[0] = (uint*)typeDefinition->ParentType;
				baseType = Type.GetTypeFromHandle(parentHandle);
			}

			// Declaring Type
			if (typeDefinition->DeclaringType != null)
			{
				RuntimeTypeHandle declaringHandle = new RuntimeTypeHandle();
				((uint**)&declaringHandle)[0] = (uint*)typeDefinition->DeclaringType;
				declaringType = Type.GetTypeFromHandle(declaringHandle);
			}

			// Element Type
			if (typeDefinition->ElementType != null)
			{
				RuntimeTypeHandle elementHandle = new RuntimeTypeHandle();
				((uint**)&elementHandle)[0] = (uint*)typeDefinition->ElementType;
				elementType = Type.GetTypeFromHandle(elementHandle);
			}
		}

		public override Type AsType()
		{
			return asType;
		}

		public override int GetArrayRank()
		{
			// We don't know so just return 1 if array, 0 otherwise
			return (IsArrayImpl() == true) ? 1 : 0;
		}

		public override Type GetElementType()
		{
			return elementType;
		}

		public override Type[] GetGenericParameterConstraints()
		{
			// No planned support
			throw new NotSupportedException();
		}

		public override Type GetGenericTypeDefinition()
		{
			// No planned support
			throw new NotSupportedException();
		}

		protected override bool HasElementTypeImpl()
		{
			return (elementType != null);
		}

		protected override bool IsArrayImpl()
		{
			return typeCode == TypeCode.Array || typeCode == TypeCode.SZArray;
		}

		protected override bool IsByRefImpl()
		{
			// We don't know so just return false
			return false;
		}

		protected override bool IsNestedImpl()
		{
			return (attributes & TypeAttributes.VisibilityMask) > TypeAttributes.Public;
		}

		protected override bool IsPointerImpl()
		{
			return typeCode == TypeCode.ManagedPointer || typeCode == TypeCode.UnmanagedPointer;
		}

		protected override bool IsPrimitiveImpl()
		{
			return (typeCode == TypeCode.Boolean
				|| typeCode == TypeCode.Char
				|| (typeCode >= TypeCode.I && typeCode <= TypeCode.I8)
				|| (typeCode >= TypeCode.U && typeCode <= TypeCode.U8)
				|| typeCode == TypeCode.R4
				|| typeCode == TypeCode.R8);
		}

		protected override bool IsValueTypeImpl()
		{
			Type thisType = AsType();
			if (thisType == ValueType || thisType == EnumType)
				return false;

			return IsSubclassOf(ValueType);
		}

		public override Type MakeArrayType()
		{
			// No planned support
			throw new NotSupportedException();
		}

		public override Type MakeArrayType(int rank)
		{
			// No planned support
			throw new NotSupportedException();
		}

		public override Type MakeByRefType()
		{
			// No planned support
			throw new NotSupportedException();
		}

		public override Type MakeGenericType(params Type[] typeArguments)
		{
			// No planned support
			throw new NotSupportedException();
		}

		public override Type MakePointerType()
		{
			// No planned support
			throw new NotSupportedException();
		}
	}
}
