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
	
	static List<string> extensions3D = new List<string> { ".obj", ".glb", ".gltf" };
	
	protected void Awake() {
		GetComponent<SelectFileDialog>().extensionsToShow = extensions3D.ToArray();
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
		} else if (ext == ".obj") {
			Debug.Log("Exporting to OBJ file: " + filePath);
		} else {
			Debug.LogError("Unknown export type: " + ext);
		}
	}
	
}
