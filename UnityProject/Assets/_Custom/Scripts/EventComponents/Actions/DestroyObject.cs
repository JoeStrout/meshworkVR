using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
	public void Destroy(GameObject gob) {
		Object.Destroy(gob);
	}
	
	public void DestroyImmediate(GameObject gob) {
		Object.DestroyImmediate(gob);
	}
	
}
