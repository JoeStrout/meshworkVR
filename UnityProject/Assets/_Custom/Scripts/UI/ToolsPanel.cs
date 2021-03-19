/*
This script manages the Tools panel, which shows all the different tools
you can choose from for each hand.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ToolsPanel : MonoBehaviour
{
	List<ToolOption> options;
	
	protected void Awake() {
		options = new List<ToolOption>(GetComponentsInChildren<ToolOption>());
	}

	public void NoteClicked(ToolOption option, bool leftHand) {
		// Update the tool buttons to show which is selected
		foreach (var opt in options) {
			if (leftHand) opt.selectedLeft = (opt == option);
			else opt.selectedRight = (opt == option);
		}
		
		// Then update the actual tools in use
		ApplyTools();
	}
	
	void ApplyTools() {
		foreach (var opt in options) {
			if (opt.selectedLeft) ApplyTool(opt, true);
			if (opt.selectedRight) ApplyTool(opt, false);
		}
	}
	
	void ApplyTool(ToolOption toolOption, bool leftHand) {
		ToolModeManager mgr = (leftHand ? GlobalRefs.instance.leftRayInteractor : GlobalRefs.instance.rightRayInteractor)
			.GetComponentInParent<ToolModeManager>();
		Tool tool = null;
		Transform toolT = mgr.transform.Find(toolOption.name);
		if (toolT != null) tool = toolT.GetComponent<Tool>();
		mgr.SelectTool(tool);
	}
}
