using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInteractionTool : Tool
{
	[Tooltip("The maximum distance at which we can interact with UI")]
	public float canApplyDistance = 0.5f;
	
	int uiLayerMask;
	
	protected void Awake() {
		uiLayerMask = LayerMask.GetMask("UI");	
	}
	
	// Return whether this tool can currently apply, i.e., we are pointed at
	// some UI object.
	public bool CanApply() {
		return Physics.Raycast(transform.position, transform.forward, canApplyDistance, uiLayerMask, QueryTriggerInteraction.Collide);
	}
}
