/*
	RotateSteady

Notes:
	The axis of rotation may be either fixed to the world
	X, Y, and Z axes, or it may be relative to the orientation
	of the object -- choose which you want via localAxes.
*/	

using UnityEngine;
using System.Collections;

public class RotateSteady : MonoBehaviour {
	// Speed and axis to rotate around, in degrees per second
	public Vector3 axis = new Vector3(0,1,0);
	public float turnRate = 180;
	
	// LocalAxes: set this to true if you want rotation that changes based
	// on which way the object is facing.  Set it to false if you want rotation
	// on the X, Y, and Z axes regardless of how the object is rotated.
	public bool localAxes = true;

	// whether we should be rotating or not
	public bool rotating = false;
	
	Rigidbody rb;
	
	protected void Awake() {
		rb = GetComponent<Rigidbody>();
	}
	
	void Update() {
		if (!rotating) return;

		float turnAngle = turnRate * Time.deltaTime;
		Vector3 turnAxis = axis;
		Quaternion q = Quaternion.AngleAxis(turnAngle, turnAxis);

		if (rb != null) {
			if (localAxes) {
				rb.MoveRotation(GetComponent<Rigidbody>().rotation * q);
			} else {
				rb.MoveRotation(q * GetComponent<Rigidbody>().rotation);
			}
		} else {
			if (localAxes) {
				transform.rotation = transform.rotation * q;
			} else {
				transform.rotation = q * transform.rotation;
			}
		}
	}
	
	
	public void StartRotating() {
		rotating = true;
	}
	
	public void StopRotating() {
		rotating = false;
	}
}