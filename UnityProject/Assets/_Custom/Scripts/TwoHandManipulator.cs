/*
This class implements manipulating (transforming) an object with two hands.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoHandManipulator
{
	Transform[] grabbers = new Transform[2];	// our two manipulators
	Transform obj;		// the object we are manipulating
	
	Vector3[] grabPos = new Vector3[2];
	Quaternion[] grabRot = new Quaternion[2];
	Vector3 objStartPos;
	Quaternion objStartRot;
	Vector3 objStartScale;
	
	public void Init(Transform grabber0, Transform grabber1, Transform obj) {
		grabbers[0] = grabber0;
		grabbers[1] = grabber1;
		this.obj = obj;
		
		grabPos[0] = grabbers[0].position;
		grabRot[0] = grabbers[0].rotation;
		grabPos[1] = grabbers[1].position;
		grabRot[1] = grabbers[1].rotation;
		objStartPos = obj.position;
		objStartRot = obj.rotation;
		objStartScale = obj.localScale;
	}
	
	public void Update() {
		// translation, using the average of the two grab positions
		Vector3 curPos = (grabbers[0].position + grabbers[1].position) * 0.5f;
		Vector3 startPos = (grabPos[0] + grabPos[1]) * 0.5f;
		Vector3 delta = curPos - startPos;
		obj.position = objStartPos + delta;
		
		// rotation
		//Quaternion startRot = Quaternion.Slerp(grabRot[0], grabRot[1], 0.5f);
		//Quaternion curRot = Quaternion.Slerp(grabbers[0].rotation, grabbers[1].rotation, 0.5f);
		//		Quaternion drot = MathUtils.FromToRotation(startRot, curRot);		

		Vector3 startAxis = grabPos[0] - startPos;
		Vector3 curAxis = grabbers[0].position - curPos;
		Quaternion drot = Quaternion.FromToRotation(startAxis, curAxis);

		// Apply the change in axis from the positions of the hands
		obj.rotation = drot * objStartRot;
		
		// And also apply a rotation around that axis, from tilting of the grabbers.
		float tiltAng0 = ToAngleRange(grabbers[0].eulerAngles.x - grabRot[0].eulerAngles.x);
		float tiltAng1 = ToAngleRange(grabbers[1].eulerAngles.x - grabRot[1].eulerAngles.x);
		// Note: the axis about which we want to rotate needs to always point the same way,
		// e.g. from left to right.  This is independent of which order we use the grabbers in.
		Vector3 axis = curAxis;
		if (grabbers[0].GetComponent<Grabber>().handTracker.leftHand) axis = -axis;
		Quaternion tilt = Quaternion.AngleAxis((tiltAng0 + tiltAng1) * 0.5f, axis);
		obj.rotation = tilt * obj.rotation;
		
		// scale
		float scaleFactor = Vector3.Distance(grabbers[0].position, grabbers[1].position)
			/ Vector3.Distance(grabPos[0], grabPos[1]);
		if (float.IsFinite(scaleFactor) && !float.IsNaN(scaleFactor)) {
			if (scaleFactor > 0.99f && scaleFactor < 1.01f) scaleFactor = 1;
			obj.localScale = objStartScale * scaleFactor;
		}
	}
	
	/// <summary>
	/// Return the given value as an angle from -180 to 180.
	/// </summary>
	/// <param name="ang"></param>
	/// <returns></returns>
	float ToAngleRange(float ang) {
		if (ang > 180) ang = Mathf.Repeat(ang, 360) - 360;
		else if (-ang > 180) ang = -(Mathf.Repeat(-ang, 360) - 360);
		return ang;
	}
}
