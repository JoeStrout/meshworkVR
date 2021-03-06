/*
This is the base class for any "tool": something you can hold in your hand,
and apply to the model via positioning and use of the tool hand buttons/stick.

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
	#pragma warning disable CS0108  // 'audio' hides inherited member 'audio' (etc.)
	
	protected HandTracker handTracker;
	protected AudioSource audio;
	
	protected virtual void Awake() {
		handTracker = transform.parent.GetComponentInChildren<Grabber>().handTracker;
		Debug.Assert(handTracker != null);		
		audio = GetComponent<AudioSource>();
	}

	public void Activate() {
		gameObject.SetActive(true);
	}
	
	public void Deactivate() {
		gameObject.SetActive(false);
	}
}
