/*
This component represents an object in space that can be grabbed
and manipulated by the user.
*/
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
public class Grabbable : MonoBehaviour
{
	public bool isScalable = true;
	public bool destructible = true;
	public bool hideInsteadOfDestroy = true;
	
	[Space(10)]
	public GameObject highlight;

	[SerializeField] ParticleSystem myPoof;
	[SerializeField] float destroyMyPoofAfter = 1f;

	public Color32 highlightWarningColor = ParseHTMLColorCode("ff0000");

	Color32 highlightDragActiveColor = ParseHTMLColorCode("feb300");
	Color32 highlightScalePossibleColor = ParseHTMLColorCode("00effe");
	Color32 highlightScaleActiveColor = ParseHTMLColorCode("0093df");

	[Space(10)]
	[SerializeField] float updateRate = 1f / 6;
	[SerializeField] bool ignoreAuthority = false; // for other players moving your stuff?!! - #TODO

	public Grabber activeGrabber;
	bool isBeingGrabbed;

	List<Grabber> activeColliders = new List<Grabber>();
	List<Grabber> activeColliders_cleanup = new List<Grabber>();
	List<Grabber> activeColliders_removal = new List<Grabber>();

	[Space(10)]
	[Header("__ DEBUGGING __")]

	public bool onTriggerStay;
	public string stayingTriggerName = "";

	Collider[] myColliders;
	Transform originalParent;
	Grabber initialGrabber;

	public MeshRenderer highlightMeshRenderer;
	public Color32 originalHighlightColor;

	Dictionary<Grabber, bool> grabberTouching = new Dictionary<Grabber, bool>();
    
	void Start() {
		myColliders = GetComponentsInChildren<Collider>();
		if (highlight != null) {
			highlight.SetActive(false);
			highlightMeshRenderer = highlight.GetComponent<MeshRenderer>();
			if (highlightMeshRenderer != null) originalHighlightColor = highlightMeshRenderer.material.color;
		}

		InvokeRepeating("InvokedUpdate", updateRate, updateRate);
		originalParent = transform.parent;
	}

	public void InvokedUpdate() {
		activeColliders.RemoveAll(item => item == null);
		activeColliders.RemoveAll(item => !item.gameObject.activeInHierarchy);

		if (highlight != null && highlight.activeSelf && activeColliders.Count == 0) {
			highlight.SetActive(false);
			highlightMeshRenderer.material.color = originalHighlightColor;
		}
		
		if (myColliders == null || myColliders.Length == 0) return;
		// Unity won't give me trigger-trigger callbacks unless we add Rigidbodies,
		// so never mind, I'll just do it myself.
		foreach (Grabber g in GlobalRefs.instance.grabbers) {
			if (!grabberTouching.ContainsKey(g)) grabberTouching[g] = false;
			if (grabberTouching[g]) {
				if (!IsTouching(g)) {
					OnTriggerExit(g.collider);
					grabberTouching[g] = false;
				}
			} else {
				if (IsTouching(g)) {
					OnTriggerEnter(g.collider);
					grabberTouching[g] = true;
				}
			}
		}
	}

	/// <summary>
	/// Return true if the given grabber is touching any of our colliders.
	/// </summary>
	bool IsTouching(Grabber g) {
		for (int i=0; i<myColliders.Length; i++) {
			if (g.IsTouching(myColliders[i])) return true;
		}
		return false;
	}

	public Bounds GetBounds() {
		if (myColliders == null || myColliders.Length == 0) return default(Bounds);
		Bounds result = myColliders[0].bounds;
		for (int i=1; i<myColliders.Length; i++) result.Encapsulate(myColliders[i].bounds);
		return result;
	}

	private void OnTriggerEnter(Collider other) {
		//Debug.Log($"{gameObject.name} OnTriggerEnter({other.gameObject.name})");
           
		Grabber myGrabber = other.GetComponent<Grabber>();

		if (myGrabber != null) {
			if (myGrabber.myGrabbable == null) myGrabber.RegisterGrabbable(this);

			if (!activeColliders.Contains(myGrabber)) activeColliders.Add(myGrabber);

			if (highlight != null) highlight.SetActive(true);
		} 
	}

	public void OnTriggerExit(Collider other) {
		//Debug.Log($"{gameObject.name} OnTriggerExit({other.gameObject.name})");

		Grabber myGrabber = other.GetComponent<Grabber>();

		if (myGrabber != null) {
			if (myGrabber.myGrabbable == this) myGrabber.UnegisterGrabbable(this);

			activeColliders.RemoveAll(item => item == myGrabber);
		}
	}


	public void ActivateSelfDestructSequence() {
		DoSelfDestruct();
	}

	protected virtual void DoSelfDestruct()
	{
		if (hideInsteadOfDestroy) gameObject.SetActive(false);
		else Destroy(gameObject);       
	}


	/// <summary>
	/// Move instantly to the given position, facing the given point (probably
	/// rotating around Y only), and become active.  This is used when a panel 
	/// is summoned via a menu.
	/// </summary>
	public void TeleportTo(Vector3 position, Vector3 facingPoint) {
		transform.position = position;
		Quaternion q = Quaternion.LookRotation(facingPoint - position);
		transform.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
		gameObject.SetActive(true);
	}



	// -- Parse &HEX color code  ---------------------------------------------------------------------------------------------------------
	//
	public static Color ParseHTMLColorCode(string hexColorCode)
	{
		if (hexColorCode.Length >= 6) {
			bool success = true;

			byte r = 255;
			byte g = 0;
			byte b = 255;
			byte a = 255;

			try { r = byte.Parse(hexColorCode.Substring(0, 2), System.Globalization.NumberStyles.HexNumber); } catch { success = false; }
			try { g = byte.Parse(hexColorCode.Substring(2, 2), System.Globalization.NumberStyles.HexNumber); } catch { success = false; }
			try { b = byte.Parse(hexColorCode.Substring(4, 2), System.Globalization.NumberStyles.HexNumber); } catch { success = false; }

			if (hexColorCode.Length >= 8) {
				try { a = byte.Parse(hexColorCode.Substring(6, 2), System.Globalization.NumberStyles.HexNumber); } catch { success = false; }
			}

			if (success) return new Color32(r, g, b, a);
		}

		return Color.magenta;
	}


}
