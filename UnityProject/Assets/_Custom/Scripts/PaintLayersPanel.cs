/*
Represents the Paint Layers panel, that shows a scrolling list of texture layers
that are combined on a particular model.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintLayersPanel : MonoBehaviour
{
	public RectTransform listContent;
	public TexLayerRow rowPrototype;

	public MeshRenderer model;

	List<TexLayerRow> rows;
	
	protected void Start() {
		LoadFromRenderer(model);
	}
	
	public void LoadFromRenderer(MeshRenderer mr) {
		LoadMaterials(mr.sharedMaterials);
	}
	
	public void LoadMaterials(Material[] mats) {
		rowPrototype.gameObject.SetActive(false);
		
		for (int i=0; i<mats.Length; i++) {
			var noob = Instantiate(rowPrototype, rowPrototype.transform.parent);
			noob.Configure(mats[i], $"Layer {i+1}");
			var rt = noob.transform as RectTransform;
			rt.anchoredPosition = -rt.sizeDelta * i;
			noob.gameObject.SetActive(true);
		}
	}
}
