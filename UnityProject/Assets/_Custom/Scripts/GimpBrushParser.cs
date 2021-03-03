/*
Code to parse GIMP brush files in .gbr and .gih format.  References:
https://docs.gimp.org/en/gimp-using-brushes.html
https://www.gimp.org/tutorials/Image_Pipes/
https://gitlab.gnome.org/GNOME/gimp/-/blob/master/devel-docs/gih.txt
https://gitlab.gnome.org/GNOME/gimp/-/blob/master/devel-docs/gbr.txt
*/
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct GimpBrush {
	public string name;			// brush name
	public Texture2D texture;
	public int spacing;			// default spacing to be used for brush, as % of brush width
}

public class GimpBrushParser
{
	/// <summary>
	/// Read a single Gimp brush from the given file, starting at the
	/// current position of the reader.
	/// </summary>
	/// <param name="br"></param>
	/// <returns></returns>
	public static GimpBrush ParseGbr(BinaryReader br) {
		uint headerSize = br.ReadUInt32();
		uint version = br.ReadUInt32();
		uint width = br.ReadUInt32();
		uint height = br.ReadUInt32();
		uint colorDepth = br.ReadUInt32();	// 1 = greyscale, 4 = RGBA
		Debug.Log($"{headerSize} {version} {width} {height} {colorDepth}");
		GimpBrush brush = new GimpBrush();
		if (version > 1) {
			uint magicNum = br.ReadUInt32();
			brush.spacing = (int)br.ReadUInt32();
		} else brush.spacing = 10;
		byte[] nameBytes = br.ReadBytes((int)(headerSize - (version==1 ? 20 : 28)));
		brush.name = System.Text.Encoding.UTF8.GetString(nameBytes).Trim('\0', '\n', ' ');
		Debug.Log($"Reading brush {brush.name.Length} {brush.name}, a {width}x{height} brush of depth {colorDepth}");
		
		brush.texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
		Color32[] pixels = new Color32[(int)(width * height)];
		int i=0;
		for (int y=0; y<height; y++) {
			for (int x=0; x<width; x++) {
				Color32 c = (colorDepth == 1 ? new Color32(255,255,255,br.ReadByte()) 
					: new Color32(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte()));
				pixels[i++] = c;
			}
		}
		brush.texture.SetPixels32(pixels);
		brush.texture.Apply();
		return brush;
	}
}

// Amazingly, .NET does not include an endian-aware binary reader.  Wowzers.
// Ref: https://stackoverflow.com/questions/8620885/
public class EndianAwareBinaryReader : BinaryReader
{
	public enum Endianness
	{
		Little,
		Big,
	}

	private readonly Endianness _endianness = Endianness.Little;

	public EndianAwareBinaryReader(Stream input) : base(input)
	{
	}

	public EndianAwareBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
	{
	}

	public EndianAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
	{
	}

	public EndianAwareBinaryReader(Stream input, Endianness endianness) : base(input)
	{
		_endianness = endianness;
	}

	public EndianAwareBinaryReader(Stream input, Encoding encoding, Endianness endianness) : base(input, encoding)
	{
		_endianness = endianness;
	}

	public EndianAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness) : base(input, encoding, leaveOpen)
	{
		_endianness = endianness;
	}

	public override short ReadInt16() => ReadInt16(_endianness);

	public override int ReadInt32() => ReadInt32(_endianness);

	public override long ReadInt64() => ReadInt64(_endianness);

	public override ushort ReadUInt16() => ReadUInt16(_endianness);

	public override uint ReadUInt32() => ReadUInt32(_endianness);

	public override ulong ReadUInt64() => ReadUInt64(_endianness);

	public short ReadInt16(Endianness endianness) => BitConverter.ToInt16(ReadForEndianness(sizeof(short), endianness), 0);

	public int ReadInt32(Endianness endianness) => BitConverter.ToInt32(ReadForEndianness(sizeof(int), endianness), 0);

	public long ReadInt64(Endianness endianness) => BitConverter.ToInt64(ReadForEndianness(sizeof(long), endianness), 0);

	public ushort ReadUInt16(Endianness endianness) => BitConverter.ToUInt16(ReadForEndianness(sizeof(ushort), endianness), 0);

	public uint ReadUInt32(Endianness endianness) => BitConverter.ToUInt32(ReadForEndianness(sizeof(uint), endianness), 0);

	public ulong ReadUInt64(Endianness endianness) => BitConverter.ToUInt64(ReadForEndianness(sizeof(ulong), endianness), 0);

	private byte[] ReadForEndianness(int bytesToRead, Endianness endianness)
	{
		var bytesRead = ReadBytes(bytesToRead);

		if ((endianness == Endianness.Little && !BitConverter.IsLittleEndian)
			|| (endianness == Endianness.Big && BitConverter.IsLittleEndian))
		{
			Array.Reverse(bytesRead);
		}

		return bytesRead;
	}
}
