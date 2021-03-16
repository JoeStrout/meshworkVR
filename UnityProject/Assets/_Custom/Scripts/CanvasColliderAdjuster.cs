/*
Adjusts the size of the BoxCollider on both this object (the Canvas), and the
parent object (usually a grabbable GameObject).
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasColliderAdjuster : MonoBehaviour
{
	protected void OnValidate() {
		AdjustColliders();
	}
	
	protected void Start() {
		AdjustColliders();
		
		Canvas canv = GetComponent<Canvas>();
		if (canv.worldCamera == null) canv.worldCamera = Camera.main;
	}
	
	[ContextMenu("Adjust Colliders")]
	public void AdjustColliders() {
		Canvas canv = GetComponent<Canvas>();
		if (canv == null) {
			Debug.LogWarning($"Canvas not found on {gameObject.name}", gameObject);
			return;
		}
		RectTransform rt = transform as RectTransform;
		
		var bc = GetComponent<BoxCollider>();
		if (bc == null) bc = gameObject.AddComponent<BoxCollider>();
		bc.size = new Vector3(rt.sizeDelta.x, rt.sizeDelta.y, 10);
		bc.center = Vector3.zero;
		
		bc = transform.parent.GetComponent<BoxCollider>();
		if (bc == null) return;	// (this is OK)
		bc.size = new Vector3(rt.sizeDelta.x * rt.localScale.x, rt.sizeDelta.y * rt.localScale.y, 10 * rt.localScale.z);
		bc.center = Vector3.zero;		
	}
}
