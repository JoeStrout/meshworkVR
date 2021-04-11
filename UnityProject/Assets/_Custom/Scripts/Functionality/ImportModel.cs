/*
This script handles importing a model into the scene as an editable object.
ToDo: consolidate this and AddReference, which have a lot in common.
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImportModel : MonoBehaviour
{
	public Dummiesman.OBJLoader objLoader;
	public UnityGLTF.GLTFComponent gltfLoader;
	public Material uvMapMat;
	public Material wireframeMat;
	
	static List<string> extensions3D = new List<string> { ".obj", ".glb", ".gltf" };
	
	public void ImportFromFile(string filePath) {
		string ext = Path.GetExtension(filePath).ToLowerInvariant();
		if (extensions3D.Contains(ext)) {
			Add3DReference(filePath);
		} else {
			Debug.Log("Unknown import file type: " + filePath);
		}
	}
	
	void Add3DReference(string filePath) {

		if (filePath.EndsWith(".obj") || filePath.EndsWith(".OBJ")) {
			objLoader.LoadAsync(filePath, (GameObject obj) => {
				if (obj != null) PostLoadSetup(obj);				
			});
		} else if (filePath.EndsWith(".glb") || filePath.EndsWith(".gltf")) {
			gltfLoader.LoadAsync(filePath, (GameObject obj) => {
				if (obj != null) PostLoadSetup(obj);				
			});
		}
		
	}
	
	void PostLoadSetup(GameObject obj) {
		Bounds b = FindBounds(obj);
		Debug.Log("Loaded " + obj.name + " with bounds " + b);
				
		// scale and position reasonably
		float scale = 1;
		if (b.extents.magnitude > 10) scale = 2f / b.extents.magnitude;
		obj.transform.position = (-b.center + Vector3.up * b.extents.y) * scale;
		obj.transform.localScale = Vector3.one * scale;
				
		// make it grabbable
		foreach (MeshFilter mf in obj.GetComponentsInChildren<MeshFilter>()) {
			mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
		}
		obj.SetLayerRecursively(LayerMask.NameToLayer("Grabbable"));
		obj.AddComponent<Grabbable>();
		
		// make it editable, and move it into the (Meshwork) scene root
		MakeEditable(obj);
		obj.transform.SetParent(GlobalRefs.instance.scene.transform);
	}
	
	Bounds FindBounds(GameObject obj) {
		Bounds b = default(Bounds);
		bool first = true;
		foreach (Renderer r in obj.GetComponentsInChildren<Renderer>()) {
			if (first) {
				b = r.bounds;
				first = false;
			} else {
				b.Encapsulate(r.bounds);
			}
		}
		return b;
	}
	
	void MakeEditable(GameObject obj) {
		foreach (var mf in obj.GetComponentsInChildren<MeshFilter>()) {
			var mr = mf.GetComponent<MeshRenderer>();
			if (mr == null) continue;
			var gob = mf.gameObject;

			// add a mesh collider
			var mc = gob.AddComponent<MeshCollider>();
			mc.sharedMesh = mf.sharedMesh;
			
			// fix the material (giving it a unique clone)
			mr.material = new Material(uvMapMat);
			
			// make it paintable
			var pt = gob.AddComponent<PaintIn3D.P3dPaintableTexture>();
			gob.AddComponent<PaintIn3D.P3dPaintable>();
			
			// add our own mesh-editing components
			var md = gob.AddComponent<MeshDisplay>();
			md.showWireframe = true;
			md.wireframeMaterial = wireframeMat;
			gob.AddComponent<MeshModel>();
		}
	}
}
