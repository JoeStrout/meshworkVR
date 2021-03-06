/*
This script goes on each of the buttons that represent tools you can choose
for either hand.  It works with a ToolsPanel component on the containing
Canvas.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolOption : MonoBehaviour, IPointerDownHandler
{
	public GameObject leftHandIcon;
	public GameObject rightHandIcon;
	public Graphic background;
	
	public Color normalBgColor = new Color(0.5f, 0.5f, 0.5f);
	public Color selectedBgColor = new Color(0.8f, 0.8f, 0.8f);
	
	public bool selectedLeft {
		get {
			return leftHandIcon.activeSelf;
		}
		set {
			leftHandIcon.SetActive(value);
			UpdateBackground();
		}
	}
	
	public bool selectedRight {
		get {
			return rightHandIcon.activeSelf;
		}
		set {
			rightHandIcon.SetActive(value);
			UpdateBackground();
		}
	}
	
	ToolsPanel owner;
	
	protected void Awake() {
		owner = GetComponentInParent<ToolsPanel>();
		Debug.Assert(owner != null);
		selectedLeft = selectedRight = false;
	}
	
	void UpdateBackground() {
		background.color = (selectedLeft || selectedRight ? selectedBgColor : normalBgColor);
	}
	
	public void OnPointerDown(PointerEventData p) {
		//Debug.Log($"PointerDown on {gameObject.name}", gameObject);
		var evtData = p as UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData;
		bool isLeft = (evtData != null && evtData.interactor == owner.leftRayInteractor);
		if (evtData == null && Input.GetKey(KeyCode.LeftShift)) isLeft = true;
		owner.NoteClicked(this, isLeft);
	}

}
