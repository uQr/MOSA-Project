﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;

namespace Mosa.Platform.x86.Intrinsic
{
	/// <summary>
	///
	/// </summary>
	internal class FrameJump : IIntrinsicPlatformMethod
	{
		#region Methods

		/// <summary>
		/// Replaces the intrinsic call site
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="typeSystem">The type system.</param>
		void IIntrinsicPlatformMethod.ReplaceIntrinsicCall(Context context, BaseMethodCompiler methodCompiler)
		{
			Operand v0 = context.Operand1;
			Operand v1 = context.Operand2;
			Operand v2 = context.Operand3;
			Operand v3 = context.GetOperand(3);

			Operand esp = Operand.CreateCPURegister(methodCompiler.TypeSystem.BuiltIn.I4, GeneralPurposeRegister.ESP);
			Operand ebp = Operand.CreateCPURegister(methodCompiler.TypeSystem.BuiltIn.I4, GeneralPurposeRegister.EBP);

			Operand eax = Operand.CreateCPURegister(methodCompiler.TypeSystem.BuiltIn.I4, GeneralPurposeRegister.EAX);
			Operand ebx = Operand.CreateCPURegister(methodCompiler.TypeSystem.BuiltIn.I4, GeneralPurposeRegister.EBX);
			Operand ecx = Operand.CreateCPURegister(methodCompiler.TypeSystem.BuiltIn.I4, GeneralPurposeRegister.ECX);

			Operand exceptionRegister = Operand.CreateCPURegister(methodCompiler.TypeSystem.BuiltIn.Object, methodCompiler.Architecture.ExceptionRegister);

			// Move all virtual registers into physical registers - necessary since stack frame pointer will change
			context.SetInstruction(X86.Mov, eax, v0);
			context.AppendInstruction(X86.Mov, ebx, v1);
			context.AppendInstruction(X86.Mov, ecx, v2);
			context.AppendInstruction(X86.Mov, exceptionRegister, v3);

			// Update the frame and stack registers
			context.AppendInstruction(X86.Mov, ebp, ecx);
			context.AppendInstruction(X86.Mov, esp, ebx);
			context.AppendInstruction(X86.Jmp, null, eax);

			// future - common code (refactor opportunity)
			context.GotoNext();

			// Remove all remaining instructions in block and clear next block list
			while (!context.IsBlockEndInstruction)
			{
				if (!context.IsEmpty)
				{
					context.SetInstruction(X86.Nop);
				}
				context.GotoNext();
			}

			var nextBlocks = context.Block.NextBlocks;

			foreach (var next in nextBlocks)
			{
				next.PreviousBlocks.Remove(context.Block);
			}

			nextBlocks.Clear();
		}

		#endregion Methods
	}
}
