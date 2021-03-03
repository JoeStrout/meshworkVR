/*
Sets the color of the current tool, and anything else that subscribes to the event.

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolColorManager : MonoBehaviour
{
	public Color color = Color.red;
	
	public ColorEvent onColorChanged;
	
	protected void Start() {
		onColorChanged.Invoke(color);
	}
	
	public void SetRed(float red) {
		color.r = red / 255f;
		onColorChanged.Invoke(color);
	}
	
	public void SetGreen(float green) {
		color.g = green / 255f;
		onColorChanged.Invoke(color);
	}
	
	public void SetBlue(float blue) {
		color.b = blue / 255f;
		onColorChanged.Invoke(color);
	}
}
