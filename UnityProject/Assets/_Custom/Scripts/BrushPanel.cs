using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushPanel : MonoBehaviour
{
	public RawImage previewImage;
	public Slider selectionSlider;
	
	protected void OnEnable() {
		if (BrushManager.brushes == null) return;
		selectionSlider.maxValue = BrushManager.brushes.Count - 1;
		Debug.Log("Updated slider max to " + selectionSlider.maxValue);
	}
	
	protected void Start() {
		UpdatePreview();
		OnEnable();
	}
	
	public void UpdatePreview() {
		var brush = BrushManager.brushes[Mathf.RoundToInt(selectionSlider.value)];
		previewImage.texture = brush.texture;
	}
}
