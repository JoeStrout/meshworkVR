/*
Attempts to figure out which controller is interacting with a UI element.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class EventTest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
	public XRRayInteractor leftRayInteractor;
	public XRRayInteractor rightRayInteractor;
	
	public void OnPointerEnter(PointerEventData p) {
		var evtData = p as UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData;
		if (evtData != null && evtData.interactor == leftRayInteractor) {
			Debug.Log("LEFT pointer enter!");
		} else {
			Debug.Log("RIGHT pointer enter (so we assume)");
		}
		
	}
	
	public void OnPointerExit(PointerEventData p) {
		var evtData = p as UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData;
		if (evtData != null && evtData.interactor == leftRayInteractor) {
			Debug.Log("LEFT pointer exit!");
		} else {
			Debug.Log("RIGHT pointer exit (so we assume)");
		}
	}
	
	public void OnPointerDown(PointerEventData p) {
		var evtData = p as UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData;
		if (evtData != null && evtData.interactor == leftRayInteractor) {
			Debug.Log("LEFT pointer down!");
		} else {
			Debug.Log("RIGHT pointer down (so we assume)");
		}
	}
}
