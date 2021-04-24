/*
Handles display options on a mesh, including wireframe.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AmazingAssets.WireframeShader;
using PaintIn3D;

public class MeshDisplay : MonoBehaviour
{
	public Material wireframeMaterial;
	
	public bool showWireframe;
	
	public Color selectionColor = Color.cyan;
	
	// keeps track of which texture layer is currently selected for painting:
	public P3dPaintableTexture selectedTexture;
	
	Mesh mesh;
	Color32[] colors32;
	
	protected void Start() {
		if (showWireframe) {
			var mainTex = GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
			
			MeshFilter mf = GetComponent<MeshFilter>();
			Debug.Assert(mf != null);
			
			Mesh baked = mf.sharedMesh.GenerateWireframeMesh(true, true);
			baked.name += " (Baked Wireframe)";
			mf.sharedMesh = baked;
			GetComponent<MeshCollider>().sharedMesh = baked;
			
			GetComponent<MeshRenderer>().material = wireframeMaterial;
			GetComponent<MeshRenderer>().material.mainTexture = mainTex;
			Debug.Log($"{gameObject.name}: Generated {baked.name} to prepare for wireframe display");
		} else {
			// ToDo: even if we're not showing wireframe, there's something we need
			// to do here to make PaintIn3D work with the layer 0 material.
			// If we don't do it, then we simply can't paint on it.
			// Probably we need to clone the material, like P3dMaterialCloner.
			Debug.Log($"{gameObject.name}: wireframe display not selected");
		}
	}
	
	public void ShiftVertexTo(Vector3 oldPos, Vector3 newPos) {
	}
	
	void EnsureColors() {
		if (mesh == null) mesh = GetComponent<MeshFilter>().sharedMesh;
		if (colors32 != null && colors32.Length == mesh.vertexCount) return;
		colors32 = mesh.colors32;
		if (colors32 == null || colors32.Length == 0) colors32 = new Color32[mesh.vertexCount];
	}

	// Return whether the given vertex, edge, or triangle is currently selected.
	public bool IsSelected(SelectionTool.Mode mode, int index) {
		EnsureColors();
		int baseTriIdx = index * 3;
		return colors32[baseTriIdx].a > 128;
	}
	
	// Select or deselect the given vertex, edge, or triangle.
	public void SetSelected(SelectionTool.Mode mode, int index, bool isSelected) {
		EnsureColors();
		Color32 colorToSet = (isSelected ? selectionColor : Color.clear);
		var tris = mesh.triangles;
		int baseTriIdx = index * 3;
		for (int i=0; i<3; i++) {
			colors32[tris[baseTriIdx + i]] = colorToSet;
		}
		mesh.colors32 = colors32;
	}
}
