using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BrushPanel : MonoBehaviour
{
	public RawImage previewImage;
	public FormatText brushNameText;
	public Slider selectionSlider;
	
	public GimpBrush currentBrush { get; private set; }
	
	public UnityEvent onBrushChanged;
	
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
		GimpBrush oldBrush = currentBrush;
		currentBrush = BrushManager.brushes[Mathf.RoundToInt(selectionSlider.value)];
		previewImage.texture = currentBrush.texture;
		brushNameText.SetString(currentBrush.name);
		if (currentBrush != oldBrush) onBrushChanged.Invoke();
	}
}
