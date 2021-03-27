/*
This script implements a button that can be pushed in VR.  It is used for each of the keys in
a virtual keyboard, and may also be used elsewhere in the world.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

#pragma warning disable 108		// suppress spurious "hides inherited" warning

public class VRPushButton : MonoBehaviour
{	
	public Transform keyObject;
	public Vector3 pressedOffset = new Vector3(0, -0.01f, 0);
	public float repeatDelay = 0.5f;
	public float repeatPeriod = 0.2f;

	public UnityEvent onPressed;		// (includes auto-repeat)
	public UnityEvent onButtonDown;
	public UnityEvent onButtonUp;

	public bool isPressed { get { return wasPressed; } }

	public string caption { 
		get { return tmPro.text; }
		set { tmPro.text = value; }
	}

	[Header("Desktop Testing Support")]
	public KeyCode keyboardKey = KeyCode.None;
	
	TextMeshPro tmPro;
	Collider collider;
	bool wasPressed = false;
	float btnDownTime;				// Time.time at which button was actually pushed down
	float nextRepeatTime;
	Vector3 unpressedPosition;
	int fingerPressing;
	
	protected void Awake() {
		collider = GetComponentInChildren<Collider>();
		unpressedPosition = keyObject.localPosition;
		tmPro = GetComponentInChildren<TextMeshPro>();
	}
	
	
	protected void Update() {
		bool isPressed = IsBeingPressed();
		if (isPressed) {
			if (!wasPressed) {
				wasPressed = true;
				keyObject.localPosition += pressedOffset;
				onButtonDown.Invoke();
				Press();
				DoHaptics(true);
				btnDownTime = Time.time;
				nextRepeatTime = btnDownTime + repeatDelay;
			} else if (repeatPeriod > 0 && Time.time > nextRepeatTime) {
				Press();
				DoHaptics(true, true);
			}
		} else if (wasPressed) {
			wasPressed = false;
			keyObject.localPosition = unpressedPosition;
			DoHaptics(false);
			onButtonUp.Invoke();
		}
	}
	
	protected void OnMouseDown() {
		Press();	// (this is just a hack for while testing in the IDE)
	}
	
	void Press() {
		onPressed.Invoke();
		if (repeatPeriod > 0) nextRepeatTime = Time.time + repeatPeriod;
	}
	
	void DoHaptics(bool press, bool repeat=false) {
		var tracker = fingerPressing == 0 ? GlobalRefs.instance.leftHandTracker : GlobalRefs.instance.rightHandTracker;
		if (tracker == null) return;
		if (repeat) {
			tracker.Vibrate(0.08f, 0.15f);
		} else if (press) {
			tracker.Vibrate(0.1f, 0.2f);
		} else {
			tracker.Vibrate(0.04f, 0.1f);
		}
	}
	
	/// <summary>
	/// Return whether this button is being pressed by the user, by any fingertip or the alternate hardware key.
	/// </summary>
	/// <returns></returns>
	bool IsBeingPressed() {
		if (Input.GetKey(keyboardKey)) return true;

		for (int i=0; i<GlobalRefs.instance.typingTips.Length; i++) {
			Transform tip = GlobalRefs.instance.typingTips[i];
			if (tip == null || !tip.gameObject.activeInHierarchy) continue;
			if (collider.ContainsPoint(tip.position)) {
				fingerPressing = i;
				if (Input.GetKeyDown(KeyCode.LeftShift)) Debug.Log(gameObject.name + " is pressed by " + tip.name, tip.gameObject);
				return true;
			}
		}
		return false;
	}
}
