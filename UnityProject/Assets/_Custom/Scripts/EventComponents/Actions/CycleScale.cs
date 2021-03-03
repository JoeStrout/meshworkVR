/*
Simple script cycles between two different local scales using an animation curve.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CycleScale : MonoBehaviour
{
	public Vector3 scaleA = Vector3.one;
	public Vector3 scaleB = Vector3.one;
	public float cycleTime = 1;
	public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public bool cycling = true;
	
	public UnityEvent onCycle;
	
	float t;

	protected void Start() {
		if (cycling && onCycle != null) onCycle.Invoke();
	}

	protected void Update() {
		if (cycling) {
			t += Time.deltaTime / cycleTime;
			if (t >= 1) {
				t = Mathf.Repeat(t, 1);
				if (onCycle != null) onCycle.Invoke();
			}
			transform.localScale = Vector3.Lerp(scaleA, scaleB, curve.Evaluate(t));
		}
	}
	
	public void StartCycling() { 
		cycling = true;
		if (t == 0 && onCycle != null) onCycle.Invoke();
	}
	public void StopCycling() { cycling = false; }
}
