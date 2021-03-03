/*
This is a subclass of Unity's XRController that patches around some bugs
and shortcomings.

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRControllerPatched : XRController
{
	protected void Start() {
		// XRController assumes its LineRenderer positions are in world space,
		// even though (1) that's stupid, and (2) it makes it hard to preview
		// your line in the editor.  So we allow it to be in local space until
		// the component starts, and then we change it to world space.
		GetComponent<LineRenderer>().useWorldSpace = true;
	}
	
	// Override ApplyControllerState to actually honor the enableInputTracking switch.
	protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
	{
		Vector3 correctPos = transform.localPosition;
		Quaternion correctRot = transform.localRotation;
		
		base.ApplyControllerState(updatePhase, controllerState);
		
		if (!enableInputTracking) {
			transform.localPosition = correctPos;
			transform.localRotation = correctRot;
		}
	}

}
