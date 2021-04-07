/*
This script adds a reference model or image to the scene from a file.

*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AddReference : MonoBehaviour
{
	public GameObject imageRefPrefab;
	public Dummiesman.OBJLoader objLoader;
	
	public void AddReferenceFromFile(string filePath) {
		if (filePath.EndsWith(".png") || filePath.EndsWith(".jpg")) {
			Add2DReference(filePath);
		} else if (filePath.EndsWith(".obj")) {
			Add3DReference(filePath);
		} else {
			Debug.Log("Unknown reference file type: " + filePath);
		}
	}
	
	void Add2DReference(string filePath) {
		byte[] data = File.ReadAllBytes(filePath);
		Texture2D tex = new Texture2D(2,2);
		tex.LoadImage(data);
		
		var inst = Instantiate(imageRefPrefab);
		var rawImage = inst.GetComponentInChildren<RawImage>();
		rawImage.texture = tex;
		
		Canvas canv = inst.GetComponentInChildren<Canvas>();
		var rt = canv.transform as RectTransform;
		rt.sizeDelta = new Vector2(2000 * tex.width/tex.height, 2000);
	}
	
	void Add3DReference(string filePath) {
		if (filePath.EndsWith(".obj") || filePath.EndsWith(".OBJ")) {
			objLoader.LoadAsync(filePath, (GameObject obj) => {
				if (obj == null) return;
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
				
			});
		}
		
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
}
