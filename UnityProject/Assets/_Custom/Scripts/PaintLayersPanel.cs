/*
Represents the Paint Layers panel, that shows a scrolling list of texture layers
that are combined on a particular model.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PaintIn3D;

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
		int i=0;
		foreach (var pt in mr.GetComponents<P3dPaintableTexture>()) {
			rows[i].paintable = pt;
			i++;
		}
		SelectLayer(rows.Count - 1);
	}
	
	public void LoadMaterials(Material[] mats) {
		rowPrototype.gameObject.SetActive(false);
		rows = new List<TexLayerRow>();
		
		for (int i=0; i<mats.Length; i++) {
			var noob = Instantiate(rowPrototype, rowPrototype.transform.parent);
			string layerName = "Layer " + (i+1);
			noob.Configure(mats[i], layerName);
			var rt = noob.transform as RectTransform;
			rt.anchoredPosition = -rt.sizeDelta * i;
			noob.gameObject.SetActive(true);
			rows.Add(noob);
		}
	}
	
	public void UpdateMaterials() {
		List<Material> materials = new List<Material>();
		foreach (var row in rows) {
			if (row.isVisible) materials.Add(row.material);
		}
		model.materials = materials.ToArray();
	}
	
	public void SelectLayer(int layer) {
		for (int i=0; i<rows.Count; i++) {
			rows[i].isSelected = (i == layer);
		}
		NoteSelectionChanged();
	}
	
	public void NoteSelectionChanged() {
		for (int i=0; i<rows.Count; i++) {
			rows[i].paintable.Group = new P3dGroup(rows[i].isSelected ? 0 : -1);
		}
	}
}
