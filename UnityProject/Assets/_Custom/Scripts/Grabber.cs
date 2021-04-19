/*
Represents one of those little balls on your controller with which
you can grab and manipulate stuff.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
	[SerializeField] Transform myGrabAnchor;
	[SerializeField] Grabber otherGrabber;
	[SerializeField] float grabActiveAt = .75f;
	[SerializeField] float releaseAt = 0.5f;
	[SerializeField] float destroyShrunkenObjectSize = .18f;


	[Space(10)]
	[Header("Public Variables")]
	public HandTracker handTracker;
	public Grabbable myGrabbable;
	public bool isGrabbing = false;
	public bool isScaling = false;
	public float myGrabbableScaleMaxSideSize = 0f;
	public bool leftHand { get { return handTracker.leftHand; } }

	[HideInInspector] public SphereCollider collider;
	
	float initialGrabScaleDistance;
	Vector3 initialGrabScale;
	float activeGrabScaleFactor;
	
	Collider[] tempColliders = new Collider[32];
	int tempCollidersCount;
	int tempCollidersFrame;	
	static LayerMask grabbableMask;

	Transform originalParent;
	public Vector3 initialGrabScalePosition;

	GameObject grabbedOrigin;

	GameObject doubleHandGrab;
	GameObject doubleHandGrab_Placer;

	protected void Awake() {
		grabbableMask = LayerMask.GetMask("Grabbable");	

		if (myGrabAnchor == null) {
			if (transform.childCount > 0) myGrabAnchor = transform.GetChild(0);
			else myGrabAnchor = transform;
		}

		collider = GetComponent<SphereCollider>();
	}


	void Update() {
		if (handTracker.grip > grabActiveAt && !isGrabbing && !isScaling && myGrabbable == null) {
			// User is trying to grab, but is not over any grabbable.
			//  Grab the scene instead... but ONLY if the other grabbable is ALSO grabbing.
			if (otherGrabber != null && otherGrabber.handTracker.grip > grabActiveAt &&
				!otherGrabber.isGrabbing && !otherGrabber.isScaling && otherGrabber.myGrabbable == null) {
				myGrabbable = GlobalRefs.instance.scene;				
			}
		}
		
		if (myGrabbable == null) {
			if (!otherGrabber || !otherGrabber.isGrabbing) return;
		}

		if (handTracker.grip > grabActiveAt) {
			if (!isGrabbing && !isScaling) {
				// Begin a grab or scale
				
				if (otherGrabber && otherGrabber.isGrabbing) {
					if (otherGrabber.myGrabbable.isScalable) { // SCALE
						BeginScale();
					}
				} else { // GRAB
					BeginGrab();
				}
			}

			if (isScaling && otherGrabber.grabbedOrigin) {
				ContinueGrabAndScale();
			}
		} else if (handTracker.grip < releaseAt) {
			if (isGrabbing) ReleaseGrab();
			if (isScaling) ReleaseScale();
		}

	}

	void BeginGrab() {
		isGrabbing = true;
		
		originalParent = myGrabbable.transform.parent; // #NOTE ...just in case it's changed...

		if (grabbedOrigin) DestroyImmediate(grabbedOrigin);

		grabbedOrigin = new GameObject();

		grabbedOrigin.name = "--> Grabbed Origin: " + name;
		grabbedOrigin.transform.SetParent(myGrabAnchor);
		grabbedOrigin.transform.localPosition = Vector3.zero;
		grabbedOrigin.transform.localEulerAngles = Vector3.zero;
		grabbedOrigin.transform.localScale = Vector3.one;

		myGrabbable.transform.SetParent(grabbedOrigin.transform, true);
		isGrabbing = true;
	}

	void BeginScale() {
		initialGrabScale = otherGrabber.grabbedOrigin.transform.localScale;

		initialGrabScalePosition = transform.position;
		otherGrabber.initialGrabScalePosition = otherGrabber.transform.position;

		initialGrabScaleDistance = Vector3.Distance(transform.position, otherGrabber.transform.position);
		isScaling = true;

		if (doubleHandGrab != null)  Destroy(doubleHandGrab);
		if (doubleHandGrab_Placer != null) Destroy(doubleHandGrab_Placer);

		doubleHandGrab = new GameObject();
		doubleHandGrab.name = "-> DoubleHandGrab";
		doubleHandGrab.transform.SetParent(otherGrabber.transform, false);
		doubleHandGrab.transform.LookAt(transform, Vector3.Lerp(transform.up, otherGrabber.transform.up, 0.5f));
		doubleHandGrab.transform.localScale = new Vector3(1, 1, initialGrabScaleDistance);

		doubleHandGrab_Placer = new GameObject();
		doubleHandGrab_Placer.name = "--> DoubleHandGrab_Placer";
		doubleHandGrab_Placer.transform.SetParent(doubleHandGrab.transform);
		doubleHandGrab_Placer.transform.position = otherGrabber.grabbedOrigin.transform.position;
		doubleHandGrab_Placer.transform.rotation = otherGrabber.grabbedOrigin.transform.rotation;
	}

	public void ContinueGrabAndScale() {
		// Do scaling and two-handed rotation

		float xGrabberDistance = Vector3.Distance(otherGrabber.gameObject.transform.position, transform.position);

		activeGrabScaleFactor = xGrabberDistance / initialGrabScaleDistance;

		otherGrabber.grabbedOrigin.transform.localScale = initialGrabScale * activeGrabScaleFactor;

		Bounds b = otherGrabber.myGrabbable.GetBounds();
		myGrabbableScaleMaxSideSize = Mathf.Max(b.size.x, b.size.y, b.size.z);

		var highlightRend = otherGrabber.myGrabbable.highlightMeshRenderer;
		if (highlightRend) {
			if (myGrabbableScaleMaxSideSize < destroyShrunkenObjectSize && myGrabbable.destructible) {
				highlightRend.material.color = otherGrabber.myGrabbable.highlightWarningColor;
			} else {
				highlightRend.material.color = otherGrabber.myGrabbable.originalHighlightColor;
			}
		}
				
		doubleHandGrab.transform.LookAt(transform, Vector3.Lerp(transform.up, otherGrabber.transform.up, .5f));
		doubleHandGrab.transform.localScale = new Vector3(1, 1, xGrabberDistance);

		otherGrabber.grabbedOrigin.transform.position = doubleHandGrab_Placer.transform.position;
		otherGrabber.grabbedOrigin.transform.rotation = doubleHandGrab_Placer.transform.rotation;
	}

	public void ReleaseGrab() {
		if (originalParent) myGrabbable.transform.SetParent(originalParent.transform, true);
		else myGrabbable.transform.parent = null;

		if (grabbedOrigin) Destroy(grabbedOrigin);

		bool isScene = myGrabbable == GlobalRefs.instance.scene;
		isGrabbing = false;
		myGrabbable = null;

		if (doubleHandGrab != null) Destroy(doubleHandGrab);
		if (doubleHandGrab_Placer != null) Destroy(doubleHandGrab_Placer);

		if (isScene && otherGrabber != null && otherGrabber.isScaling) {
			otherGrabber.ReleaseScale();
		}
		
	}
	
	void ReleaseScale() {
		isScaling = false;
		
		var grabbable = (otherGrabber.myGrabbable == null ? myGrabbable : otherGrabber.myGrabbable);
		if (grabbable == null) return;
		
		if (myGrabbableScaleMaxSideSize < destroyShrunkenObjectSize && grabbable.destructible) {
			if (otherGrabber.myGrabbable != null) otherGrabber.myGrabbable.ActivateSelfDestructSequence();
			else if (myGrabbable != null) myGrabbable.ActivateSelfDestructSequence();
		
			otherGrabber.myGrabbable = null;
			otherGrabber.isGrabbing = false;
			otherGrabber.isScaling = false;
		}
		
		myGrabbable = null;
		isGrabbing = false;
		isScaling = false;

		if (grabbable == GlobalRefs.instance.scene && otherGrabber != null && otherGrabber.isGrabbing) {
			otherGrabber.ReleaseGrab();
		}

	}

	public bool IsTouching(Collider otherCollider) {
		if (Time.frameCount > tempCollidersFrame) {
			Vector3 center = transform.TransformPoint(collider.center);
			float radius = collider.radius * transform.lossyScale.y;
			tempCollidersCount = Physics.OverlapSphereNonAlloc(center, radius, tempColliders,
				grabbableMask, QueryTriggerInteraction.Collide);
		}
		
		int idx = System.Array.IndexOf(tempColliders, otherCollider);
		return idx >= 0 && idx < tempCollidersCount;
	}

	public void RegisterGrabbable(Grabbable incoming) {
		myGrabbable = incoming;
	}

	public void UnegisterGrabbable(Grabbable incoming) {
		myGrabbable = null;
	}

	private void OnDisable() {
		if (myGrabbable) myGrabbable.OnTriggerExit(collider);

		myGrabbable = null;
		isGrabbing = false;
		isScaling = false;
	}


}
