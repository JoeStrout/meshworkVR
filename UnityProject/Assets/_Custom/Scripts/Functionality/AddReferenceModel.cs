/*
This script adds a built-in reference model to the scene.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddReferenceModel : MonoBehaviour
{
	public GameObject prefab;
	
	public void AddReference() {
		var inst = Instantiate(prefab);
		
		
	}
}
