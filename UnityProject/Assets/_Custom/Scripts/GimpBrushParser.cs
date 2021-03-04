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

[System.Serializable]
public class GimpBrush {
	public string name;			// brush name
	public List<Texture2D> textures;
	public int spacing;			// default spacing to be used for brush, as % of brush width
	public Texture2D texture {
		get {
			if (textures == null || textures.Count < 1) return null;
			if (textures.Count == 1) return textures[0];
			return textures[textures.Count/2];
		}
	}
	
	public bool isValid {
		get { return texture != null && texture.width > 0 && texture.height > 0; }
	}
}

public static class GimpBrushParser
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
		GimpBrush brush = new GimpBrush();
		if (version > 1) {
			uint magicNum = br.ReadUInt32();
			brush.spacing = (int)br.ReadUInt32();
		} else brush.spacing = 10;
		byte[] nameBytes = br.ReadBytes((int)(headerSize - (version==1 ? 20 : 28)));
		brush.name = System.Text.Encoding.UTF8.GetString(nameBytes).Trim('\0', '\n', ' ');
		
		brush.textures = new List<Texture2D>();
		brush.textures.Add(new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false));
		Color32[] pixels = new Color32[(int)(width * height)];
		for (int y=(int)height-1; y>=0; y--) {
			int i=(int)(y * width);
			for (int x=0; x<width; x++) {
				Color32 c = (colorDepth == 1 ? new Color32(255,255,255,br.ReadByte()) 
					: new Color32(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte()));
				pixels[i++] = c;
			}
		}
		brush.textures[0].SetPixels32(pixels);
		brush.textures[0].Apply();
		return brush;
	}
	
	/// <summary>
	/// Read a multi-image brush in Gimp Image Hose (.gih) format.
	/// </summary>
	public static GimpBrush ParseGih(BinaryReader br) {
		// First line: name of brush.
		string name = ReadUTF8TillDelimiter(br);
		// Second line: number of gbr brushes, plus some other metadata
		string[] info = ReadUTF8TillDelimiter(br).Split(' ');
		int gbrCount;
		if (!int.TryParse(info[0], out gbrCount)) return null;
		
		var result = new GimpBrush();
		result.name = name;
		result.textures = new List<Texture2D>();
		
		// Now, just read a bunch of gbr files, catted together.
		// As we go we'll total up the spacing value, so we can 
		// use the average spacing as the spacing of the hose.
		int totalSpacing = 0;
		for (int i=0; i<gbrCount; i++) {
			var gbr = ParseGbr(br);
			if (gbr != null && gbr.isValid) {
				result.textures.Add(gbr.texture);
				totalSpacing += gbr.spacing;
			}
		}
		result.spacing = totalSpacing / gbrCount;
		return result;
	}
	
	/// <summary>
	/// Load a brush from a TextAsset in either .gbr or .gih format.
	/// </summary>
	public static GimpBrush LoadBrush(TextAsset brushAsset) {
		Stream s = new MemoryStream(brushAsset.bytes);
		var reader = new EndianAwareBinaryReader(s, EndianAwareBinaryReader.Endianness.Big);
		GimpBrush brush;
		if (brushAsset.name.EndsWith(".gih")) brush = ParseGih(reader);
		else brush = GimpBrushParser.ParseGbr(reader);
		reader.Close();
		s.Close();
		return brush;
	}
	
	public static string ReadUTF8TillDelimiter(BinaryReader br, char delim='\n') {
		var bytes = new List<byte>();
		while (true) {
			byte b = br.ReadByte();
			if (b == '\n') break;
			bytes.Add(b);
		}
		return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
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
