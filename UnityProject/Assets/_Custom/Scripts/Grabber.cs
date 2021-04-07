/*
Represents one of those little balls on your controller with which
you can grab and manipulate stuff.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
	public static List<Grabber> instances = new List<Grabber>();
	
	public HandTracker handTracker;
	
	public Vector3 center { get { return transform.position; } }
	public float radius { get { return collider.radius * transform.lossyScale.x; } }
	public float grabValue { get { return forcedGrab ? 1f : handTracker.grip; } }
	public float trigger { get { return handTracker.trigger; } }
	public bool isGrabbing { get { return grabValue >= 0.5f; } }
	public bool justGrabbed { get; private set; }
	public bool justReleased { get; private set; }
	
	bool forcedGrab;	
	float lastGrabValue = 0.5f;
	SphereCollider collider;
	
	Collider[] tempColliders = new Collider[32];
	static LayerMask grabbableMask;
	
	protected void Awake() {
		instances.Add(this);
		collider = GetComponent<SphereCollider>();
		grabbableMask = LayerMask.GetMask("Grabbable");
	}
	
	protected void OnDestroy() {
		instances.Remove(this);
	}
	
	protected void Update() {
		justGrabbed = grabValue >= 0.55f && lastGrabValue < 0.55f;
		justReleased = grabValue < 0.45f && lastGrabValue >= 0.45f;
		lastGrabValue = grabValue;
		
		int count = Physics.OverlapSphereNonAlloc(center, radius, tempColliders,
			grabbableMask, QueryTriggerInteraction.Collide);
		for (int i=0; i<count; i++) {
			var grabbable = tempColliders[i].GetComponentInParent<Grabbable>();
			if (grabbable != null) grabbable.NoteGrabberOverThis(this);
		}
		
		// Note that actual grabbing of grabbables is handled in Grabbable itself
		// (which allows us to grab multiple objects at once if they are close together).
		// But if you grab empty space, possibly manipulating the scene, we handle
		// that here.
		if (count == 0 && justGrabbed) {
			if (GlobalRefs.instance.scene.isGrabbed) {
				GlobalRefs.instance.scene.StretchWith(this);
			} else {
				GlobalRefs.instance.scene.GrabBy(this, true);	// require second hand in this case
			}
		}
	}
	
	[ContextMenu("Force Grab")]
	void ForceGrab() {
		forcedGrab = true;
	}
	
	[ContextMenu("Release Force Grab")]
	void ReleaseForceGrab() {
		forcedGrab = false;
	}
}
