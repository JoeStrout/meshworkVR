/*
This script handles exporting the scene to a file.
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExportModel : MonoBehaviour
{
	public TMP_InputField fileNameField;
	public Listbox fileList;
	
	static List<string> extensions3D = new List<string> { ".obj", ".glb" };
	
	protected void Awake() {
		GetComponent<SaveFileDialog>().extensionsToShow = extensions3D.ToArray();
	}
	
	public void ExportToFile(string filePath) {
		string ext = Path.GetExtension(filePath).ToLowerInvariant();
		if (ext.Length == 0 || ext.Length > 4) {
			// Looks like no extension was given.  Add a default.
			filePath += ".glb";
			ext = ".glb";
		}
		if (ext == ".glb") {
			// Export to GLB format
			Debug.Log("Exporting to GLB file: " + filePath);
			SaveSceneToGLB(filePath);
		} else if (ext == ".obj") {
			Debug.Log("Exporting to OBJ file: " + filePath);
			SaveSceneToOBJ(filePath);
		} else {
			Debug.LogError("Unknown export type: " + ext);
		}
	}
	
	public void SaveSceneToGLB(string filePath) {
		var options = new UnityGLTF.ExportOptions();
		var exporter = new UnityGLTF.GLTFSceneExporter(ObjectsToExport(), options);
		string dirPath = Path.GetDirectoryName(filePath);
		string fileName = Path.GetFileName(filePath);
		exporter.SaveGLB(dirPath, fileName);
		Debug.Log("Wrote file to: " + filePath);
	}
	
	public void SaveSceneToOBJ(string filePath) {
		ObjExporter.ExportToFile(ObjectsToExport(), filePath);
		Debug.Log("Wrote file to: " + filePath);
	}
	
	Transform[] ObjectsToExport() {
		var objects = new List<Transform>();
		for (int i=0; i<GlobalRefs.instance.scene.transform.childCount; i++) {
			Transform t = GlobalRefs.instance.scene.transform.GetChild(i);
			if (t.GetComponent<MeshModel>() != null) {
				objects.Add(t);
			}
		}
		return objects.ToArray();
	}
}
