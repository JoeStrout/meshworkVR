using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRManager : MonoBehaviour
{
	public StringEvent onLogMessage;
	
	protected void Awake() {
		Application.logMessageReceived += HandleLog;

		var devices = new List<UnityEngine.XR.InputDevice>();		
		UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.Head, devices);
		if (devices.Count == 0) {
			Debug.Log("Found no head devices!");
		} else {
			Debug.Log("Found head device: " + devices[0].name + " with tracking " + XRDevice.GetTrackingSpaceType());
		}


		XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
		Debug.Log("After setting tracking to roomscale, it's now " + XRDevice.GetTrackingSpaceType());
		// Note: once you set thet tracking space to RoomScale, the default automagic camera tracking
		// is no longer correct (too high by a factor of 2 or so).  You need to add a TrackedPoseDriver
		// to the camera; then it works correctly.
		
		// See also: https://forum.unity.com/threads/oculus-quest-wrong-floor-position.702422/):
	}

	void HandleLog(string logString, string stackTrace, LogType type) {
		string color = "white";
		if (type == LogType.Warning) color = "yellow";
		else if (type == LogType.Error || type == LogType.Exception) color = "red";
		onLogMessage.Invoke($"<color={color}>[{Time.frameCount}] {logString}</color>");
		if (type == LogType.Exception) {
			var stackLines = stackTrace.Split(new char[] {'\n'});
			onLogMessage.Invoke($"<color=grey>{stackLines[0]}</color>");
		}
	}

}
