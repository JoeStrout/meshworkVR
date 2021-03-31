/*
This script adds an image (Texture2D) as a reference image in the scene.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddReferenceImage : MonoBehaviour
{
	public Texture2D image;
	public GameObject prefab;
	
	protected void OnValidate() {
		// If we have an icon (because this script is on a button that has an icon),
		// set our texture to match the icon texture.
		var rawImage = GetComponentInChildren<RawImage>();
		if (rawImage is Texture2D) image = (Texture2D)(rawImage.texture);
	}
	
	public void AddReference() {
		var inst = Instantiate(prefab);
		var rawImage = inst.GetComponentInChildren<RawImage>();
		rawImage.texture = image;
	}
}
