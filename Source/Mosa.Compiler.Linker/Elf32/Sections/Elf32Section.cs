﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Simon Wollwage (rootnode) <kintaro@think-in-co.de>
 */

using System;
using System.IO;
using Mosa.Compiler.Common;

namespace Mosa.Compiler.Linker.Elf32.Sections
{
	/// <summary>
	/// 
	/// </summary>
	public class Elf32Section : Mosa.Compiler.Linker.LinkerSection
	{
		private readonly DataConverter LittleEndianBitConverter = DataConverter.LittleEndian;

		/// <summary>
		/// 
		/// </summary>
		protected Elf32SectionHeader header = new Elf32SectionHeader();
		/// <summary>
		/// 
		/// </summary>
		protected MemoryStream sectionStream;

		/// <summary>
		/// Initializes a new instance of the <see cref="Elf32Section"/> class.
		/// </summary>
		/// <param name="kind">The kind of the section.</param>
		/// <param name="name">The name.</param>
		/// <param name="virtualAddress">The virtualAddress.</param>
		public Elf32Section(SectionKind kind, string name, IntPtr virtualAddress)
			: base(kind, name, virtualAddress)
		{
			header = new Elf32SectionHeader();
			header.Name = Elf32StringTableSection.AddString(name);
			sectionStream = new MemoryStream();
		}

		/// <summary>
		/// Gets the length of the section in bytes.
		/// </summary>
		/// <value>The length of the section in bytes.</value>
		public override long Length
		{
			get
			{
				return sectionStream.Length;
			}
		}

		/// <summary>
		/// Gets the header.
		/// </summary>
		/// <value>The header.</value>
		public Elf32SectionHeader Header
		{
			get
			{
				return header;
			}
		}

		/// <summary>
		/// Allocates the specified size.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <param name="alignment">The alignment.</param>
		/// <returns></returns>
		public Stream Allocate(int size, int alignment)
		{
			// Do we need to ensure a specific alignment?
			if (alignment > 1)
				InsertPadding(alignment);

			return sectionStream;
		}

		/// <summary>
		/// Writes the specified fs.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public virtual void Write(BinaryWriter writer)
		{
			Header.Offset = (uint)writer.BaseStream.Position;
			sectionStream.WriteTo(writer.BaseStream);
		}

		/// <summary>
		/// Writes the header.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public virtual void WriteHeader(BinaryWriter writer)
		{
			Header.Size = (uint)Length;
			Header.Write(writer);
		}

		/// <summary>
		/// Patches the section at the given offset with the specified value.
		/// </summary>
		/// <param name="offset">The offset.</param>
		/// <param name="linkType">Type of the link.</param>
		/// <param name="value">The value.</param>
		public void ApplyPatch(long offset, LinkType linkType, long value)
		{
			long pos = sectionStream.Position;
			sectionStream.Position = offset;

			// Apply the patch
			switch (linkType & LinkType.SizeMask)
			{
				case LinkType.I1:
					sectionStream.WriteByte((byte)value);
					break;

				case LinkType.I2:
					sectionStream.Write(LittleEndianBitConverter.GetBytes((ushort)value), 0, 2);
					break;

				case LinkType.I4:
					sectionStream.Write(LittleEndianBitConverter.GetBytes((uint)value), 0, 4);
					break;

				case LinkType.I8:
					sectionStream.Write(LittleEndianBitConverter.GetBytes(value), 0, 8);
					break;
			}

			sectionStream.Position = pos;
		}

		#region Internals

		/// <summary>
		/// Pads the stream with zeros until the specific alignment is reached.
		/// </summary>
		/// <param name="alignment">The alignment.</param>
		private void InsertPadding(int alignment)
		{
			long address = VirtualAddress.ToInt64() + sectionStream.Length;
			int pad = (int)(alignment - (address % alignment));
			sectionStream.Write(new byte[pad], 0, pad);
		}

		#endregion // Internals
	}
}
