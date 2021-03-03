/*
This script causes the attached item to float up and fade out.
Used for damage pop-ups, etc.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class FloatAndFade : MonoBehaviour
{
	public float floatHeight = 0.5f;
	public float duration = 1;
	public AnimationCurve floatCurve;
	public AnimationCurve fadeCurve;
	
	public bool playOnEnable = true;
	public bool destroyWhenDone = true;
	
	public UnityEvent onPlay;
	public UnityEvent onFinished;

	Vector3 startPos;
	float startTime = 0;
	bool isPlaying;
	
	// Things to fade:
	TextMeshPro[] tmPros;
	
	protected void OnEnable() {
		if (playOnEnable) Play();
	}
	
	public void Play() {
		startPos = transform.position;
		startTime = Time.time;
		isPlaying = true;
		
		tmPros = GetComponentsInChildren<TextMeshPro>();
		// ToDo: gather other kinds of things to fade as needed.

		if (onPlay != null) onPlay.Invoke();
	}
	
	protected void Update() {
		if (isPlaying) {
			float t = Mathf.Clamp01((Time.time - startTime) / duration);
			Sample(t);
			if (t >= 1) {
				isPlaying = false;
				if (onFinished != null) onFinished.Invoke();
				if (destroyWhenDone) Destroy(gameObject);
			}
		}
	}
	
	protected void Reset() {
		floatCurve = new AnimationCurve();
		var kf = new Keyframe(0, 0);
		kf.outTangent = 1;
		floatCurve.AddKey(kf);
		kf = new Keyframe(1, 1);
		kf.inTangent = 0;
		floatCurve.AddKey(kf);
		
		fadeCurve = new AnimationCurve();
		kf = new Keyframe(0, 0);
		kf.outTangent = 0;
		fadeCurve.AddKey(kf);
		kf = new Keyframe(0.2f, 0);
		kf.outTangent = kf.inTangent = 0;
		fadeCurve.AddKey(kf);
		kf = new Keyframe(1, 1);
		kf.inTangent = 2;
		fadeCurve.AddKey(kf);
	}
	
	void Sample(float t) {
		transform.position = startPos + Vector3.up * floatCurve.Evaluate(t);
		float alpha = 1f - fadeCurve.Evaluate(t);
		foreach (var tmp in tmPros) tmp.color = ColorWithAlpha(tmp.color, alpha);
		// ToDo: fade other kinds of things as needed.
	}
	
	static Color ColorWithAlpha(Color c, float alpha) {
		c.a = alpha;
		return c;
	}
	
	[ContextMenu("Test at 0%")]
	void TestAt0() {
		if (startPos == Vector3.zero) Play();
		Sample(0);
	}
	
	[ContextMenu("Test at 30%")]
	void TestAt30() {
		if (startPos == Vector3.zero) Play();
		Sample(0.3f);
	}
	
	[ContextMenu("Test at 60%")]
	void TestAt60() {
		if (startPos == Vector3.zero) Play();
		Sample(0.6f);
	}
	
	[ContextMenu("Test at 90%")]
	void TestAt90() {
		if (startPos == Vector3.zero) Play();
		Sample(0.9f);
	}

	[ContextMenu("Test at 100%")]
	void TestAt100() {
		if (startPos == Vector3.zero) Play();
		Sample(1f);
	}

}
