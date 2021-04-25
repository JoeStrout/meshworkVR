﻿/*
Handles display options on a mesh, including wireframe.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
			Debug.Log($"{gameObject.name}: Generated {baked.name} with {baked.vertexCount} vertices to prepare for wireframe display");
		} else {
			// ToDo: even if we're not showing wireframe, there's something we need
			// to do here to make PaintIn3D work with the layer 0 material.
			// If we don't do it, then we simply can't paint on it.
			// Probably we need to clone the material, like P3dMaterialCloner.
			Debug.Log($"{gameObject.name}: wireframe display not selected");
		}
		
		GetComponent<MeshModel>().LoadMesh();
	}
	
	public void Rebake() {
		MeshFilter mf = GetComponent<MeshFilter>();
		mesh = mf.sharedMesh.GenerateWireframeMesh(true, true);
		mf.sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
		GetComponent<MeshModel>().LoadMesh();
	}
	
	void EnsureColors() {
		if (mesh == null) mesh = GetComponent<MeshFilter>().sharedMesh;
		if (colors32 != null && colors32.Length == mesh.vertexCount) return;
		Debug.Log($"Reassigning colors32 ({colors32}) from mesh ({mesh.colors32})");
		colors32 = mesh.colors32;
		if (colors32 == null || colors32.Length != mesh.vertexCount) {
			colors32 = new Color32[mesh.vertexCount];
			Debug.Log($"Created fresh colors32 array of length {colors32.Length}");
		}
	}

	// Return whether the given vertex, edge, or triangle is currently selected.
	public bool IsSelected(SelectionTool.Mode mode, int index) {
		EnsureColors();
		if (mode == SelectionTool.Mode.Face) {
			int baseTriIdx = index * 3;
			return colors32[baseTriIdx].a > 128 && colors32[baseTriIdx+1].a > 128 && colors32[baseTriIdx+2].a > 128;
		} else if (mode == SelectionTool.Mode.Vertex) {
			return colors32[index].a > 128;
		}
		return false;
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
