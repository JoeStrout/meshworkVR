/*
Represents one row of a Texture (Paint) Layers list.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TexLayerRow : MonoBehaviour
{
	public RawImage image;
	public FormatText nameText;
	public Toggle visToggle;
	
	public void Configure(Material mat, string name, bool visible=true) {
		image.material = mat;
		nameText.SetString(name);
		visToggle.isOn = visible;
	}
}
