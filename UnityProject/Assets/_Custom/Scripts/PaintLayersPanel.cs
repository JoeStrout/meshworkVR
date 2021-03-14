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
	public Material newMaterialPrototype;

	public MeshRenderer model;

	List<TexLayerRow> rows;
	int nextNewLayerNum;
	
	protected void Start() {
		LoadFromRenderer(model);
	}
	
	public void LoadFromRenderer(MeshRenderer mr) {
		LoadMaterials(mr.sharedMaterials);
		foreach (var pt in mr.GetComponents<P3dPaintableTexture>()) {
			int slot = pt.Slot.Index;
			rows[slot].paintable = pt;
		}
		SelectLayer(rows.Count - 1);
	}
	
	public void LoadMaterials(Material[] mats) {
		rowPrototype.gameObject.SetActive(false);
		rows = new List<TexLayerRow>();
		
		for (int i=0; i<mats.Length; i++) {
			AddRow(mats[i], "Layer " + (i+1));
		}
		nextNewLayerNum = rows.Count + 1;
	}
	
	void AddRow(Material mat, string name) {
		var noob = Instantiate(rowPrototype, rowPrototype.transform.parent);
		noob.Configure(mat, name);
		var rt = noob.transform as RectTransform;
		rt.anchoredPosition = -rt.sizeDelta * rows.Count;
		noob.gameObject.SetActive(true);
		rows.Add(noob);		
	}
	
	void AdjustRowPositions() {
		for (int i=0; i<rows.Count; i++) {
			var rt = rows[i].transform as RectTransform;
			rt.anchoredPosition = -rt.sizeDelta * i;			
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
		// set the group to 0 for the selected PaintableTexture, and -1 for all others
		int selIdx = -1;
		for (int i=0; i<rows.Count; i++) {
			rows[i].paintable.Group = new P3dGroup(rows[i].isSelected ? 0 : -1);
			if (rows[i].isSelected) selIdx = i;
		}
		
		// ...which isn't working great.  So let's try:
		foreach (var tool in PaintSprayTool.instances) {
			var painter = tool.GetComponent<P3dPaintDecal>();
			painter.TargetTexture = rows[selIdx].paintable;
		}
	}
	
	public void AddLayer() {
		var texture = new Texture2D(512, 512);
		texture.ClearPixels(Color.clear);
		Material m = new Material(newMaterialPrototype);
		m.mainTexture = texture;		
		AddRow(m, "Layer " + (nextNewLayerNum++));
		UpdateMaterials();
		
		var pt = model.gameObject.AddComponent<P3dPaintableTexture>();
		pt.Slot = new P3dSlot(rows.Count-1, "_MainTex");
		rows[rows.Count-1].paintable = pt;
		pt.Activate();
		
		SelectLayer(rows.Count - 1);
	}
	
	public void DeleteLayer() {
		// Loop down to (but not including) 0 -- we don't want to delete the last layer.
		for (int i=rows.Count-1; i>0; i--) {
			var row = rows[i];
			if (row.isSelected) {
				Material m = rows[i].material;
				Destroy(rows[i].paintable);
				rows.RemoveAt(i);
				UpdateMaterials();
				Destroy(m);
				Destroy(row.gameObject);
				SelectLayer(i < rows.Count ? i : rows.Count-1);
				break;
			}
		}
		AdjustRowPositions();
	}
}
