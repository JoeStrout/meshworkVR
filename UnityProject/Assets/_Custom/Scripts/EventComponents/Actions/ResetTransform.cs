using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResetTransform : MonoBehaviour
{
	public UnityEvent onReset;
	
	Vector3 localPosition;
	Quaternion localRotation;
	Vector3 localScale;
		
	protected void Awake() {
		localPosition = transform.localPosition;
		localRotation = transform.localRotation;
		localScale = transform.localScale;
	}
	
	[ContextMenu("Reset Transform Now")]
	public void ResetNow() {
		Debug.Log("Resetting " + gameObject.name + " to " + transform.localPosition, gameObject);
		transform.localPosition = localPosition;
		transform.localRotation = localRotation;
		transform.localScale = localScale;
		if (onReset != null) onReset.Invoke();
	}
	
	public void ResetOnNextFrame() {
		Invoke("ResetNow", 0.01f);
	}
}
