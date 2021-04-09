/*
This script interfaces with the VR hardware (or a game controller used
as a substitute for desktop testing) for one hand (left or right).
Place this on its own little object under the VRManager, and give it 
references to the things it needs, and you should be good to go!
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandTracker : MonoBehaviour
{
	public bool leftHand;
	
	public float trigger { get; private set; }
	public float grip { get; private set; }
	public Vector2 thumbStick { get; private set; }		// or on Vive, touch position on the thumb disk

	public bool isTracking { get; private set; }
	public Vector3 localPosition { get; private set; }
	public Quaternion localRotation { get; private set; }

	public Transform handTransform;	

	#if UNITY_EDITOR
	[Space(10)]
	[Header("Debugging")]
	public bool fakeTrigger = false;
	[Range(0,1)] public float fakeTriggerValue = 0;
	public bool fakeGrip = false;
	[Range(0,1)] public float fakeGripValue = 0;	
	#endif

	//public Grabber grabber;

	public enum Button {
		Trigger = 0,
		Grip = 1,
		X = 2,		// (or on Vive, I guess some press on the thumb disc? maybe down/left?)
		Y = 3,		// (on Vive, maybe press up/right?),
		Start = 4,
		StickPress = 5
	}
	const int kQtyButtons = 6;
	
	XRNode node;
	InputDevice device;
	bool deviceFound = false;

	bool appHasFocus = true;

	bool[] curButtonStates = new bool[kQtyButtons];
	bool[] prevButtonStates = new bool[kQtyButtons];
	
	protected void Awake() {
		node = (leftHand ? XRNode.LeftHand : XRNode.RightHand);
	}
	
	protected void OnEnable() {
		InputTracking.trackingAcquired += TrackingAcquired;
		InputTracking.trackingLost += TrackingLost;
	}
	
	protected void OnDisable() {
		InputTracking.trackingAcquired -= TrackingAcquired;
		InputTracking.trackingLost -= TrackingLost;
	}
	
	protected void OnApplicationFocus(bool focus) {
		appHasFocus = focus;
	}

	void TrackingAcquired(XRNodeState state) {
		if (state.nodeType == node) isTracking = true;
	}
	
	void TrackingLost(XRNodeState state) {
		if (state.nodeType == node) isTracking = false;
	}

	
	void Update() {
		float lastTrigger = trigger;
		System.Array.Copy(curButtonStates, prevButtonStates, kQtyButtons);
		
		if (!deviceFound) {
			var devices = new List<UnityEngine.XR.InputDevice>();		
			UnityEngine.XR.InputDevices.GetDevicesAtXRNode(node, devices);
			if (devices.Count > 0) {
				device = devices[0];
				deviceFound = true;
				isTracking = true;
			}
		}
		if (deviceFound) {
			bool b;
			float f = 0;
			trigger = 0;
			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out f) && f > 0) {
				trigger = f;
			}
		
			grip = 0;
			//if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out b) && b) {
			//	grip = 1;
			//} else grip = 0;

			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out f) && f > 0) {
				grip = f;
			}

			Vector2 stick;
			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out stick)) {
				thumbStick = stick;
			}
			
			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out b)) {
				curButtonStates[(int)Button.X] = b;
			}
			
			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out b)) {
				curButtonStates[(int)Button.Y] = b;
			}
			
			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out b)) {
				curButtonStates[(int)Button.Start] = b;
			}
			
			if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out b)) {
				curButtonStates[(int)Button.StickPress] = b;
			}
		}
		
		if (isTracking) {
		
			// ToDo: update the following to use TryGetFeatureValue (with CommonUsages.devicePosition etc.) instead.
			//localPosition = InputTracking.GetLocalPosition(node);
			//localRotation = InputTracking.GetLocalRotation(node);
			Vector3 localPosition;
			device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out localPosition);
			Quaternion localRotation;
			device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out localRotation);
						
			if (handTransform != null) {
				handTransform.localPosition = localPosition;
				handTransform.localRotation = localRotation;
			}
			
			// Commented out the above to try a TrackedPoseDriver instead; it behaves the exact
			// same way.
		}
		
		if (!deviceFound || !isTracking) DoGameControllerStandin();
		//else Debug.Log("No controller standin for you!");
		
		#if UNITY_EDITOR
		if (fakeTrigger) trigger = fakeTriggerValue;
		if (fakeGrip) grip = fakeGripValue;
		#endif
		
		curButtonStates[(int)Button.Trigger] = (trigger > 0.5f);
		curButtonStates[(int)Button.Grip] = (grip > 0.5f);
		
	}
	
	public bool GetButton(Button b) {
		return curButtonStates[(int)b];
	}
	
	public bool GetButtonDown(Button b) {
		return curButtonStates[(int)b] && !prevButtonStates[(int)b];
	}
	
	public bool GetButtonUp(Button b) {
		return prevButtonStates[(int)b] && !curButtonStates[(int)b];
	}
	
	/// <summary>
	/// There's no VR tracking available, so let's use a game controller
	/// to substitute for the sake of testing.
	/// 
	/// Yeah, this code is hacky.  It's only for testing and varies with my test hardware.
	/// </summary>
	void DoGameControllerStandin() {
		
		//Debug.Log("DoGameControllerStandin; left grip "+ Input.GetAxis("Left Grip") + ", stick " + Input.GetAxis("Left Thumb Stick Vertical"));
		if (!appHasFocus) return;	// only for the focused app!
		
		string side = (leftHand ? "Left" : "Right");
		trigger = Input.GetAxis(side + " Trigger");
		//for (int i=1; i<18; i++) {
		//	if (Input.GetKey("joystick button " + i)) Debug.Log("joystick button " + i);
		//}
		grip = (Input.GetAxis(side + " Grip") + 1) * 0.5f;	// (remap -1:1 to 0:1)
		
		if (!leftHand) {
			if (Input.GetKey(KeyCode.PageDown)) grip = 1;
			if (Input.GetKey(KeyCode.PageUp)) trigger = 1;
		} else {
			if (Input.GetKey(KeyCode.End)) grip = 1;
			if (Input.GetKey(KeyCode.Home)) trigger = 1;			
		}
		//if (grip == 1) Debug.Log($"{gameObject.name} grip: 1", gameObject);
		
		thumbStick = new Vector2(
			Input.GetAxis(side + " Thumb Stick Horizontal"),
			Input.GetAxis(side + " Thumb Stick Vertical"));
		thumbStick += new Vector2(
			Input.GetAxis("Horizontal"),
			Input.GetAxis("Vertical"));
		
		if (leftHand) {
			curButtonStates[(int)Button.X] = Input.GetButton("X") || Input.GetKey("page down");
			curButtonStates[(int)Button.Y] = Input.GetButton("Y") || Input.GetKey("page up");
		} else {
			curButtonStates[(int)Button.X] = Input.GetButton("A");
			curButtonStates[(int)Button.Y] = Input.GetButton("B");
		}
		curButtonStates[(int)Button.Start] = Input.GetKey("home");
		//if (curButtonStates[(int)Button.Start]) Debug.Log("Start pressed");
	}
	
	public void Vibrate(float duration = 0.2f, float amplitude = 0.5f) {
		UnityEngine.XR.HapticCapabilities capabilities;
		if (device == null || !device.TryGetHapticCapabilities(out capabilities)
			|| !capabilities.supportsImpulse) return;
	
		uint channel = 0;
		device.SendHapticImpulse(channel, amplitude, duration);
	}
}
