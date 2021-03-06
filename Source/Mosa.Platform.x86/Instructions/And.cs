﻿// Copyright (c) MOSA Project. Licensed under the New BSD License.

using Mosa.Compiler.Framework;
using System;

namespace Mosa.Platform.x86.Instructions
{
	/// <summary>
	/// Representations the x86 and instruction.
	/// </summary>
	public sealed class And : TwoOperandInstruction
	{
		#region Data Members

		private static readonly LegacyOpCode R_C = new LegacyOpCode(new byte[] { 0x81 }, 4);
		private static readonly LegacyOpCode M_C = R_C;
		private static readonly LegacyOpCode R_M = new LegacyOpCode(new byte[] { 0x23 });
		private static readonly LegacyOpCode R_R = R_M;
		private static readonly LegacyOpCode M_R = new LegacyOpCode(new byte[] { 0x21 });

		#endregion Data Members

		#region Methods

		/// <summary>
		/// Computes the opcode.
		/// </summary>
		/// <param name="destination">The destination operand.</param>
		/// <param name="source">The source operand.</param>
		/// <param name="third">The third operand.</param>
		/// <returns></returns>
		internal override LegacyOpCode ComputeOpCode(Operand destination, Operand source, Operand third)
		{
			if (destination.IsCPURegister)
			{
				if (third.IsCPURegister) return R_R;
				if (third.IsConstant) return R_C;
			}

			throw new ArgumentException(@"No opcode for operand type.");
		}

		#endregion Methods
	}
}
