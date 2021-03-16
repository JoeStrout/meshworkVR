/*
Represents one row of a Texture (Paint) Layers list.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PaintIn3D;

public class TexLayerRow : MonoBehaviour
{
	public RawImage image;
	public FormatText nameText;
	public Toggle visToggle;
	public Material material;
	public P3dPaintableTexture paintable;
	
	Toggle selToggle;
	
	public bool isVisible {
		get { return visToggle.isOn; }
		set { visToggle.isOn = value; }
	}
	
	public bool isSelected {
		get { return selToggle.isOn; }
		set { selToggle.isOn = value; }
	}
	
	protected void Awake() {
		selToggle = GetComponent<Toggle>();
	}
	
	public void Configure(Material mat, string name, bool visible=true) {
		gameObject.name = name;
		this.material = mat;
		image.texture = mat.mainTexture;
		nameText.SetString(name);
		visToggle.isOn = visible;
	}
	
}
