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
	
	protected void Awake() {
		var mainTex = GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
		
		MeshFilter mf = GetComponent<MeshFilter>();
		Debug.Assert(mf != null);
		
		Mesh baked = mf.sharedMesh.GenerateWireframeMesh(true, true);
		baked.name += " (Baked Wireframe)";
		mf.sharedMesh = baked;
		GetComponent<MeshCollider>().sharedMesh = baked;
		
		GetComponent<MeshRenderer>().material = wireframeMaterial;
		GetComponent<MeshRenderer>().material.mainTexture = mainTex;
	
	}
	
	public void ShiftVertexTo(Vector3 oldPos, Vector3 newPos) {
	}
}
