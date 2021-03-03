using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ToggleActive : MonoBehaviour {
	
	[Tooltip("Object to toggle; if null, toggles self")]
	public GameObject target;
	
	public void Toggle() {
		GameObject gob = target;
		if (gob == null) gob =  gameObject;
		gob.SetActive(!gob.activeSelf);
	}
}
