using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AmazingAssets.WireframeShader;

public class FaceTintTest : MonoBehaviour
{
	public Color testColor;
	
	protected void Start() {
		var mesh = GetComponent<MeshFilter>().sharedMesh;
		GetComponent<MeshFilter>().sharedMesh = mesh.GenerateWireframeMesh(true, true);
	}
	
	void Update() {
		var mesh = GetComponent<MeshFilter>().sharedMesh;
		var tris = mesh.triangles;
		Color32[] vertColors = new Color32[mesh.vertexCount];
		for (int i=0; i<6; i++) vertColors[tris[i]] = testColor;
		mesh.colors32 = vertColors;
	}
}
