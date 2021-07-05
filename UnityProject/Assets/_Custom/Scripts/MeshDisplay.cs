/*
Handles display options on a mesh, including wireframe.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AmazingAssets.WireframeShader;
using PaintIn3D;

public enum MeshEditMode {
	Vertex,
	Edge,
	Face
}

public class MeshDisplay : MonoBehaviour
{
	public Material wireframeMaterial;
	public Material editEdgeMaterial;
	
	public bool showWireframe;
	
	public Color selectionColor = Color.cyan;
	public MeshEditMode mode { get; private set; }
	
	// keeps track of which texture layer is currently selected for painting:
	public P3dPaintableTexture selectedTexture;
	
	Mesh mesh;
	Color32[] colors32;
	bool[] edgeSelected;	// separate boolean array to keep track of which edges are currently selected
	
	MeshRenderer meshRenderer;
	Material edgeModeMat;	// material instance when editing in edge mode
	Material otherModeMat;	// material instance in any other mode
	
	protected void Start() {

		meshRenderer = GetComponent<MeshRenderer>();
		var mainTex = meshRenderer.sharedMaterial.mainTexture;

		edgeModeMat = new Material(editEdgeMaterial);
		edgeModeMat.mainTexture = mainTex;
		
		otherModeMat = new Material(wireframeMaterial);
		otherModeMat.mainTexture = mainTex;
		
		if (showWireframe) {
			
			MeshFilter mf = GetComponent<MeshFilter>();
			Debug.Assert(mf != null);
			
			Mesh baked = mf.sharedMesh.GenerateWireframeMesh(true, false);// true);
			baked.name += " (Baked Wireframe)";
			mf.sharedMesh = baked;
			GetComponent<MeshCollider>().sharedMesh = baked;
			
			meshRenderer.material = otherModeMat;
			Debug.Log($"{gameObject.name}: Generated {baked.name} with {baked.vertexCount} vertices to prepare for wireframe display");
			EnsureColors();
			mesh.colors32 = colors32;
		} else {
			// ToDo: even if we're not showing wireframe, there's something we need
			// to do here to make PaintIn3D work with the layer 0 material.
			// If we don't do it, then we simply can't paint on it.
			// Probably we need to clone the material, like P3dMaterialCloner.
			Debug.Log($"{gameObject.name}: wireframe display not selected");
		}
		
		var model = GetComponent<MeshModel>();
		if (model != null) model.LoadMesh();
	}
	
	public void Rebake() {
		MeshFilter mf = GetComponent<MeshFilter>();
		mesh = mf.sharedMesh.GenerateWireframeMesh(true, true);
		mf.sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
		var model = GetComponent<MeshModel>();
		if (model != null) model.LoadMesh();
		EnsureColors();
		mesh.colors32 = colors32;
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

	/// <summary>
	/// Make sure we have an edgeSelected array that is the correct length
	/// for the number of triangles in our mesh.
	/// </summary>
	void EnsureEdgeSelArray() {
		int properEdgeCount = mesh.triangles.Length * 3;
		if (edgeSelected == null) edgeSelected = new bool[properEdgeCount];
		else if (edgeSelected.Length != properEdgeCount) System.Array.Resize(ref edgeSelected, properEdgeCount);
	}
	
	// Ensure our mesh has the UV2 array set up properly for edge coloring.
	void EnsureUV2() {
		// Assign UVs in a special pattern per triangle, which the shader can
		// use to figure out which side of the triangle it's on.
		if (mesh.uv2 != null && mesh.uv2.Length == mesh.vertexCount) return;
		var tris = mesh.triangles;
		var uv2 = new Vector2[mesh.vertexCount];
		for (int trinum=0; trinum<tris.Length; trinum += 3) {
			uv2[tris[trinum]] = Vector2.zero;
			uv2[tris[trinum+1]] = new Vector2(1,0);
			uv2[tris[trinum+2]] = new Vector2(0.5f, 1);
		}
		mesh.uv2 = uv2;
	}

	public void SetMode(MeshEditMode mode, bool forceApply=false) {
		if (mode != this.mode || forceApply) {
			Material correctMat = (mode == MeshEditMode.Edge ? edgeModeMat : otherModeMat);
			Debug.Log($"Switching material on {gameObject.name} to {correctMat.name}");
			meshRenderer.material = correctMat;
			this.mode = mode;
		}
	}

	// Return whether the given vertex, edge, or triangle is currently selected.
	public bool IsSelected(MeshEditMode mode, int index) {
		SetMode(mode);
		EnsureColors();
		if (mode == MeshEditMode.Face) {
			int baseTriIdx = index * 3;
			return colors32[baseTriIdx].a > 128 && colors32[baseTriIdx+1].a > 128 && colors32[baseTriIdx+2].a > 128;
		} else if (mode == MeshEditMode.Edge) {
			EnsureEdgeSelArray();
			return edgeSelected[index];
		} else if (mode == MeshEditMode.Vertex) {
			return colors32[index].a > 128;
		}
		return false;
	}
	
	// Select or deselect the given vertex, edge, or triangle.
	public void SetSelected(MeshEditMode mode, int index, bool isSelected) {
		Debug.Log($"SetSelected({mode}, {index}, {isSelected})", gameObject);
		SetMode(mode);
		EnsureColors();
		if (mode == MeshEditMode.Edge) {
			EnsureEdgeSelArray();
			EnsureUV2();
			edgeSelected[index] = isSelected;
			// Edge highlighting works by setting red, green, and blue of the face color
			// to indicate what combination of the three edges is selected.
			Color32 c = new Color32();
			int baseIndex = (index / 3) * 3;
			if (edgeSelected[baseIndex]) c.r = 255;
			if (edgeSelected[baseIndex+1]) c.g = 255;
			if (edgeSelected[baseIndex+2]) c.b = 255;
			colors32[baseIndex] = colors32[baseIndex+1] = colors32[baseIndex+2] = c;
		} else {
			Color32 colorToSet = (isSelected ? selectionColor : Color.clear);
			var tris = mesh.triangles;
			int baseTriIdx = index * 3;
			for (int i=0; i<3; i++) {
				colors32[tris[baseTriIdx + i]] = colorToSet;
			}
		}
		mesh.colors32 = colors32;
	}
	
	// Clear the selection.  Return true if we had a selection to clear,
	// false if nothing was selected anyway.
	public bool DeselectAll(MeshEditMode mode) {
		SetMode(mode);
		EnsureColors();
		bool clearedAny = false;
		if (mode == MeshEditMode.Edge) {
			Color32 black = new Color32(0,0,0,255);
			for (int i=0; i<edgeSelected.Length; i++) if (edgeSelected[i]) {
				edgeSelected[i] = false;
				int baseIndex = (i / 3) * 3;
				colors32[baseIndex] = colors32[baseIndex+1] = colors32[baseIndex+2] = black;
				clearedAny = true;
			}			
		} else {
			Color32 clear = Color.clear;
			for (int i=0; i<colors32.Length; i++) {
				clearedAny = clearedAny || colors32[i].a > 128;
				colors32[i] = clear;
			}
		}
		if (!clearedAny) return false;
		mesh.colors32 = colors32;
		return true;
	}
}
