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
	public XRRayInteractor leftRayInteractor;
	public XRRayInteractor rightRayInteractor;

	List<ToolOption> options;
	
	protected void Awake() {
		options = new List<ToolOption>(GetComponentsInChildren<ToolOption>());
	}

	public void NoteClicked(ToolOption option, bool leftHand) {
		foreach (var opt in options) {
			if (leftHand) opt.selectedLeft = (opt == option);
			else opt.selectedRight = (opt == option);
		}
	}
}
