/*
This component represents an object in space that can be grabbed
and manipulated by the user.
*/
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
public class Grabbable : MonoBehaviour
{
	
	public StringEvent onGrabOver;
	public StringEvent onGrabExit;

	protected Collider[] colliders;
	
	protected bool wasOver;	// true when at least one grabber was over it last frame

	// when grabbed, the following apply:
	protected Grabber primaryGrab;			// hand that actually grabbed us
	protected Vector3 primaryGrabLocalPos;	// local position of the grabber when grabbed
	protected Quaternion grabRotOffset;		// rotation offset from primary grabber
	protected bool requiresTwoHands = false;// if true, position/rotation also requires a second hand
	protected Grabber secondaryGrab;		// other hand that may be used for stretching
	protected float secondaryDist;			// distance between primary and secondary grabbers at start of stretch
	protected Vector3 baseScale;			// local scale of transform at start of stretch
	
	public bool isGrabbed { get { return primaryGrab != null; } }

	protected Dictionary<Grabber, bool> currentGrabbersTouching;
	protected Dictionary<Grabber, bool> previousGrabbersTouching;

	protected void OnValidate() {
		if (gameObject.layer != LayerMask.NameToLayer("Grabbable")) {
			Debug.LogError($"{gameObject.name} has Grabbable, but is not in the Grabbable layer", gameObject);
		}
	}

	protected void Awake() {
		colliders = GetComponentsInParent<Collider>();
		currentGrabbersTouching = new Dictionary<Grabber, bool>();
		previousGrabbersTouching = new Dictionary<Grabber, bool>();
	}

	protected void LateUpdate() {
		if (isGrabbed) UpdateWhileGrabbed();
		else UpdateHover();
	}
		
	void UpdateWhileGrabbed() {
		if (primaryGrab.justReleased) {
			Release();
			return;
		}
		
		BeforeGrabbedUpdate();
		
		if (!requiresTwoHands || secondaryGrab != null) {
			// update rotation
			transform.rotation = primaryGrab.transform.rotation * grabRotOffset;
			
			// update position
			Vector3 dPos = primaryGrab.transform.position - transform.TransformPoint(primaryGrabLocalPos);
			transform.position += dPos;
		}
		
		// check for stretching
		if (secondaryGrab == null) {
			foreach (Grabber g in Grabber.instances) {
				if (g == primaryGrab) continue;
				if (g.justGrabbed) {
					// Start stretching with this other grabber
					StretchWith(g);
					break;
				}
			}
		} else if (!secondaryGrab.isGrabbing) {
			secondaryGrab = null;
		} else {
			float curDist = Vector3.Distance(primaryGrab.transform.position, secondaryGrab.transform.position);
			float factor = curDist / secondaryDist;
			if (factor > 0.99f && factor < 1.01f) factor = 1f;	// snap
			transform.localScale = baseScale * factor;
		}
		
		AfterGrabbedUpdate();
	}
	
	protected virtual void BeforeGrabbedUpdate() {}
	protected virtual void AfterGrabbedUpdate() {}
	
	void UpdateHover() {
		// Determine if any grabber is hovering over us, by checking currentGrabbersTouching
		bool isOver = false;
		foreach (var kv in currentGrabbersTouching) {
			if (!kv.Value) continue;
			isOver = true;
			var grabber = kv.Key;
			if (grabber.justGrabbed && primaryGrab == null) GrabBy(grabber);
		}
		if (isOver != wasOver) {
			if (isOver) {
				onGrabOver.Invoke($"Grabber over {gameObject.name}");
			} else {
				onGrabExit.Invoke($"Grabber exits {gameObject.name}");
			}
			wasOver = isOver;
		}
		
		// Copy current into previous grabbers touching, and clear current.
		foreach (Grabber g in Grabber.instances) {
			bool b = false;
			previousGrabbersTouching[g] = (currentGrabbersTouching.TryGetValue(g, out b) && b);
			currentGrabbersTouching[g] = false;
		}
	}
	
	public void GrabBy(Grabber grabber, bool requireTwoHands=false) {
		Debug.Log($"Grabbed by {grabber.gameObject.name}");
		primaryGrab = grabber;
		secondaryGrab = null;
		this.requiresTwoHands = requireTwoHands;
		primaryGrabLocalPos = transform.InverseTransformPoint(grabber.center);
		grabRotOffset = Quaternion.Inverse(grabber.transform.rotation) * transform.rotation;
	}
	
	public void StretchWith(Grabber grabber) {
		secondaryGrab = grabber;
		secondaryDist = Vector3.Distance(primaryGrab.transform.position, secondaryGrab.transform.position);
		baseScale = transform.localScale;
	}
	
	void Release() {
		BeforeRelease();
		Debug.Log($"Released by {primaryGrab.gameObject.name}");
		primaryGrab = secondaryGrab = null;
		AfterRelease();
	}
	
	protected virtual void BeforeRelease() {}
	protected virtual void AfterRelease() {}
	
	/// <summary>
	/// This method is called by a Grabber that is touching this Grabbable.
	/// Note that it may get called several times on one frame if we have
	/// multiple colliders.
	/// </summary>
	public void NoteGrabberOverThis(Grabber grabber) {
		currentGrabbersTouching[grabber] = true;
	}
	
}
