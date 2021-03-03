using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ChangeScale : MonoBehaviour {
	#region Public Properties
	[Tooltip("Scale to change to")]
	public Vector3 targetScale = Vector3.one;

	[Tooltip("Time (in seconds) to spend on the transition")]
	public float transitionTime = 3;

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	Vector3 originalScale;
	bool transitioning = false;
	Vector3 startScale = Vector3.one;
	Vector3 endScale = Vector3.one;
	float startTime = 0;

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Start() {
		originalScale = transform.localScale;
	}
	
	void Update() {
		if (!transitioning) return;
		float t = (Time.time - startTime) / transitionTime;
		transform.localScale = Vector3.Lerp(startScale, endScale, t);
		if (t >= 1) transitioning = false;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods

	public void TransitionToTargetScale() {
		startScale = transform.localScale;
		endScale = targetScale;
		startTime = Time.time;
		transitioning = true;
	}

	public void TransitionToOriginalScale() {
		startScale = transform.localScale;
		endScale = originalScale;
		startTime = Time.time;
		transitioning = true;
	}

	public void JumpToTargetScale() {
		transform.localScale = targetScale;
		transitioning = false;
	}

	public void JumpToOriginalScale() {
		transform.localScale = originalScale;
		transitioning = false;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	#endregion
}
