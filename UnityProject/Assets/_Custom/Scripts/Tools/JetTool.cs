/*
This tool is a small hand jet that lets you fly around the work space.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetTool : Tool
{
	public Transform engine;
	public ParticleSystem jet;
	public Transform xrRig;
	
	public bool forceApply = false;
	
	public float topSpeed = 5f;
	public float accel = 3f;
	public float stopAccel = 10f;
	public float flipSpeed = 720f;
	
	public float turnSpeed = 180f;
	
	Vector3 velocity;
	
	protected void Update() {
		base.Update();
		
		// When A/X button is pressed, reverse the engine; otherwise point it normally
		float ang = engine.localEulerAngles.y;
		float targetAng = handTracker.GetButton(HandTracker.Button.X) ? 180 : 0;
		if (Input.GetKey(KeyCode.Tab)) targetAng = 180;
		engine.localEulerAngles = new Vector3(0, Mathf.MoveTowards(ang, targetAng, flipSpeed * Time.deltaTime), 0);
		
		// Apply thrust
		float thrust = handTracker.trigger;
		if (forceApply) thrust = 1;
		UpdateFX(thrust);

		velocity = Vector3.MoveTowards(velocity, Vector3.zero, stopAccel * Time.deltaTime);
		velocity -= engine.forward * thrust * accel * Time.deltaTime;
		
		if (velocity != Vector3.zero) xrRig.position += velocity;
		
		// Turn with left/right on joystick.
		float joyx = handTracker.thumbStick.x;
		float turnRate = 0;
		if (joyx > 0.1f) turnRate = Mathf.InverseLerp(0.1f, 1f, joyx) * turnSpeed;
		else if (-joyx > 0.1f) turnRate = Mathf.InverseLerp(0.1f, 1f, -joyx) * (-turnSpeed);
		if (turnRate != 0) {
			Transform camT = Camera.main.transform;
			// A little tricky -- we need to move the XRRig so that the camera stays put.
			// So, just do the rotation, then adjust position accordingly.
			Vector3 camPos = camT.position;
			xrRig.Rotate(0, turnRate * Time.deltaTime, 0, Space.Self);
			xrRig.position += camPos - camT.position;
		}
		
	}
	
	void UpdateFX(float thrust) {
		if (thrust < 0.001f) {
			if (audio.isPlaying) audio.Stop();
			if (jet.isPlaying) jet.Stop();
		} else {
			audio.volume = thrust;
			audio.pitch = Mathf.Lerp(0.7f, 1.2f, thrust);
			if (!audio.isPlaying) audio.Play();
			
			var mod = jet.main;
			mod.startSize = new ParticleSystem.MinMaxCurve(
				Mathf.Lerp(0.1f, 1f, thrust), 
				Mathf.Lerp(0.5f, 1.5f, thrust));
			if (!jet.isPlaying) jet.Play();
		}
	}
}
