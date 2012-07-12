/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Phil Garcia (tgiphil) <phil@thinkedge.com>
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Mosa.Compiler.InternalTrace;
using Mosa.Compiler.TypeSystem;

namespace Mosa.Compiler.Framework.Stages
{
	/// <summary>
	/// Schedules compilation of types/methods.
	/// </summary>
	public sealed class CompilationScheduler : ICompilationScheduler2
	{

		#region Data Members

		private readonly List<RuntimeType> typesAllocated = new List<RuntimeType>();
		private readonly List<RuntimeMethod> methodsInvoked = new List<RuntimeMethod>();

		private readonly HashSet<RuntimeType> typeScheduled = new HashSet<RuntimeType>();
		private readonly HashSet<RuntimeMethod> methodScheduled = new HashSet<RuntimeMethod>();
		private readonly Queue<RuntimeMethod> methodQueue = new Queue<RuntimeMethod>();

		private readonly ITypeSystem typeSystem;

		private readonly bool compileAllMethods;

		#endregion // Data Members

		public CompilationScheduler(ITypeSystem typeSystem)
		{
			this.typeSystem = typeSystem;
			compileAllMethods = true;
		}

		#region ICompilationScheduler members

		void ICompilationScheduler2.TrackTypeAllocated(RuntimeType type)
		{
			typesAllocated.Add(type);

			if (compileAllMethods)
				CompileType(type);
		}

		void ICompilationScheduler2.TrackMethodInvoked(RuntimeMethod method)
		{
			methodsInvoked.Add(method);

			if (compileAllMethods)
				CompileMethod(method);			
		}

		void ICompilationScheduler2.TrackFieldReferenced(RuntimeField field)
		{
			if (compileAllMethods)
				CompileType(field.DeclaringType);
		}

		#endregion // ICompilationSchedulerStage members

		private void CompileType(RuntimeType type)
		{
			// Can not compile an open generic type
			if (type.ContainsOpenGenericParameters)
				return;

			if (typeScheduled.Contains(type))
				return;

			//
			typeScheduled.Add(type);
			foreach (var method in type.Methods)
				CompileMethod(method);
		}

		private void CompileMethod(RuntimeMethod method)
		{
			// Can not compile an (open) generic method 
			if (method.IsGeneric)
				return;

			// can not compile a native method
			if (method.IsNative)
				return;

			if (methodScheduled.Contains(method))
				return;

			methodScheduled.Add(method);
			methodQueue.Enqueue(method);
		}

		RuntimeMethod ICompilationScheduler2.GetMethodToCompile()
		{
			if (methodQueue.Count == 0)
				return null;

			return methodQueue.Dequeue();
		}

	}
}