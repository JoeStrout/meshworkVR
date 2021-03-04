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
	
	int curTexIndex = 0;
	float nextTexTime = 0;
	
	protected void OnEnable() {
		if (BrushManager.brushes == null) return;
		selectionSlider.maxValue = BrushManager.brushes.Count - 1;
		Debug.Log("Updated slider max to " + selectionSlider.maxValue);
	}
	
	protected void Start() {
		UpdatePreview();
		OnEnable();
	}
	
	protected void Update() {
		if (Time.time > nextTexTime && currentBrush != null && currentBrush.textures.Count > 1) {
			curTexIndex = (curTexIndex + 1) % (currentBrush.textures.Count);
			previewImage.texture = currentBrush.textures[curTexIndex];
			nextTexTime = Time.time + 0.2f;
		}
	}
	
	public void UpdatePreview() {
		GimpBrush oldBrush = currentBrush;
		currentBrush = BrushManager.brushes[Mathf.RoundToInt(selectionSlider.value)];
		previewImage.texture = currentBrush.texture;
		brushNameText.SetString(currentBrush.name);
		if (currentBrush != oldBrush) onBrushChanged.Invoke();
		curTexIndex = -1;
		nextTexTime = Time.time + 0.2f;
	}
}
