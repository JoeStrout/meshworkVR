/*
Handles display options on a mesh, including wireframe.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AmazingAssets.WireframeShader;

public class MeshDisplay : MonoBehaviour
{
	public Material wireframeMaterial;
	
	public bool showWireframe;
	
	protected void Awake() {
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
		} else {
			// ToDo: even if we're not showing wireframe, there's something we need
			// to do here to make PaintIn3D work with the layer 0 material.
			// If we don't do it, then we simply can't paint on it.
			// Probably we need to clone the material, like P3dMaterialCloner.
		}
	}
	
	public void ShiftVertexTo(Vector3 oldPos, Vector3 newPos) {
	}
}
