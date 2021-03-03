// ToDo: refactor this to use MoveTowards, so that if you change
// your direction midstream, we don't jump back to the beginning.

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class TransformAtoB : MonoBehaviour {
	[Serializable]
	public class LocalTransform {
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one;
		public Quaternion rotation = Quaternion.identity;

		public LocalTransform(Transform t=null) {
			if (t != null) Capture(t);
		}

		public static LocalTransform Lerp(LocalTransform a, LocalTransform b, float t) {
			LocalTransform result = new LocalTransform();
			result.position = Vector3.Lerp(a.position, b.position, t);
			result.scale = Vector3.Lerp(a.scale, b.scale, t);
			result.rotation = Quaternion.Lerp(a.rotation, b.rotation, t);
			return result;
		}


		public static void LerpAndApply(LocalTransform a, LocalTransform b, float t, Transform applyTo) {
			applyTo.localPosition = Vector3.Lerp(a.position, b.position, t);
			applyTo.localScale = Vector3.Lerp(a.scale, b.scale, t);
			applyTo.localRotation = Quaternion.Lerp(a.rotation, b.rotation, t);
		}
		
		public void Capture(Transform t) {
			position = t.localPosition;
			scale = t.localScale;
			rotation = t.localRotation;
		}

		public void Apply(Transform t) {
			t.localPosition = position;
			t.localScale = scale;
			t.localRotation = rotation;
		}
	}

	#region Public Properties
	public LocalTransform transformA = new LocalTransform();
	public LocalTransform transformB = new LocalTransform();

	public float transitionTime = 5;

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	enum Mode { 
		Done,
		GoingAtoB,
		GoingBtoA
	};
	Mode mode = Mode.Done;
	float startTime = 0;

	#endregion
	//--------------------------------------------------------------------------------
	#region Editor Support
	[ContextMenu ("Store current as A")]
	public void StoreCurrentAsA() {
		transformA.Capture(transform);
	}
	
	[ContextMenu ("Store current as B")]
	public void StoreCurrentAsB() {
		transformB.Capture(transform);
	}
	
	[ContextMenu ("Go to A")]
	public void JumpToA() {
		transformA.Apply(transform);
		mode = Mode.Done;
	}
	
	[ContextMenu ("Go to B")]
	public void JumpToB() {
		transformB.Apply(transform);
		mode = Mode.Done;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Update() {
		if (mode == Mode.Done) return;
		float t = (Time.time - startTime) / transitionTime;
		if (transitionTime <= 0) t = 1;
		if (mode == Mode.GoingAtoB) {
			LocalTransform.LerpAndApply(transformA, transformB, t, transform);
		} else {
			LocalTransform.LerpAndApply(transformB, transformA, t, transform);
		}
		if (t >= 1) mode = Mode.Done;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	public void TransitionToA() {
		mode = Mode.GoingBtoA;
		startTime = Time.time;
	}

	public void TransitionToB() {
		mode = Mode.GoingAtoB;
		startTime = Time.time;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	#endregion
}
