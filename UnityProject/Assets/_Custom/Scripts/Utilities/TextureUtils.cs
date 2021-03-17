/*
This is a module of utilities for working with Unity textures (especially Texture2D).
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureUtils {

	public static Texture2D Subsection(this Texture2D tex, int left, int bottom, int width, int height) {
		if (left + width > tex.width) width = tex.width - left;
		if (bottom + height > tex.height) height = tex.height - bottom;
		if (left < 0) { width += left; left = 0; }
		if (bottom < 0) { height += bottom; bottom = 0; }
		if (width <= 0 || height <= 0) return null;
		
		Texture2D result = new Texture2D(width, height, tex.format, false);
		CopyPixels(tex, left, bottom, width, height, result, 0, 0);
		result.wrapMode = TextureWrapMode.Clamp;
		result.filterMode = FilterMode.Point;
		return result;
	}
	
	public static void CopyPixels(Texture2D src, int srcLeft, int srcBottom, int width, int height,
								Texture2D dest, int destLeft, int destBottom, bool apply=true) 
	{
		Color32[] srcPixels = src.GetPixels32();
		Color32[] destPixels = new Color32[width*height];
		int srcIndex = srcBottom * src.width + srcLeft;
		int destIndex = 0;
		for (int y=0; y<height; y++) {
			System.Array.Copy(srcPixels, srcIndex, destPixels, destIndex, width);
			srcIndex += src.width;
			destIndex += width;
		}
		dest.SetPixels32(destLeft, destBottom, width, height, destPixels);
		if (apply) dest.Apply();
	}
	
	public static void ClearPixels(this Texture2D tex, Color32 toColor, bool apply=true) {
		int n = tex.width * tex.height;
		Color32[] pixels = new Color32[n];
		for (int i=0; i<n; i++) pixels[i] = toColor;
		tex.SetPixels32(pixels);
		if (apply) tex.Apply();
	}
}
