using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInteractionTool : Tool
{
	[Tooltip("The maximum distance at which we can interact with UI")]
	public float canApplyDistance = 0.5f;
	
	int uiLayerMask;
	
	protected override void Awake() {
		base.Awake();
		uiLayerMask = LayerMask.GetMask("UI");	
	}
	
	// Return whether this tool can currently apply, i.e., we are pointed at
	// some UI object.
	public bool CanApply() {
		bool hit = Physics.Raycast(transform.position, transform.forward, canApplyDistance, uiLayerMask, QueryTriggerInteraction.Collide);
		Debug.DrawLine(transform.position, transform.position + transform.forward * canApplyDistance,
			hit ? Color.green : Color.cyan, 0.1f);
		return hit;		
	}
}
