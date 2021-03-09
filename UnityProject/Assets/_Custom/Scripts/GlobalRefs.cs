/*
This component provides handy global references to things in the scene
or in the project hierarchy.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRefs : MonoBehaviour
{
	public static GlobalRefs instance { get; private set; }
	
	public Grabbable scene;
	public Transform debugAxes;
	
	protected void Awake() {
		instance = this;
	}
	
	protected void OnDestroy() {
		if (instance == this) instance = null;
	}
}
