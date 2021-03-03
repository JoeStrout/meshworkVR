/* This script fires events when its renderer becomes visible or not-visible to
some camera (based solely on the viewing frustrum, i.e., ignoring occlusion).
Handy to, for example, disable that expensive extra camera rendering frames for 
a mirror when the mirror itself isn't in view. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InFrustumTrigger : MonoBehaviour {

	[Tooltip("Camera to test visibility for; if null, uses Camera.main")]
	new public Camera camera;	
	
	[Tooltip("Renderer to use the bounds of; if null, finds first renderer on this object")]
	new public Renderer renderer;

	[Tooltip("Fired when this object enters the camera's viewing frustum")]
	public UnityEvent onBecameVisible;
	
	[Tooltip("Fired when this object leaves the camera's viewing frustum")]
	public UnityEvent onBecameInvisible;
	
	bool wasVisible;

	protected void Awake() {
		if (renderer == null) renderer = GetComponentInChildren<Renderer>();
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera == null ? Camera.main : camera);
		bool isVisible = GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
		SendEvents(isVisible);
	}
	
	void Update() {
		Camera cam = camera;
		if (cam == null) cam = Camera.main;
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
		bool isVisible = GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
		if (isVisible != wasVisible) SendEvents(isVisible);
	}
	
	void SendEvents(bool isVisible) {
		if (isVisible) onBecameVisible.Invoke();
		else onBecameInvisible.Invoke();
		wasVisible = isVisible;		
	}
}
