using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class Texture2DEvent : UnityEvent<Texture2D> {}

public class BrushTest : MonoBehaviour
{
	public TextAsset brushAsset;
	public RawImage display;
	
	public Texture2DEvent onBrushLoaded;
	
	protected void Start() {
		Stream s = new MemoryStream(brushAsset.bytes);
		var reader = new EndianAwareBinaryReader(s, EndianAwareBinaryReader.Endianness.Big);
		var brush = GimpBrushParser.ParseGbr(reader);
		display.texture = brush.texture;
		onBrushLoaded.Invoke(brush.texture);
	}

}
