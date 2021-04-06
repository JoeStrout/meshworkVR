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
		Debug.Log("To-Do!");
		
	}
}
