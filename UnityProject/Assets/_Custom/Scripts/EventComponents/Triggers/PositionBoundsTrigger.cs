using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PositionBoundsTrigger : MonoBehaviour {
	#region Public Properties
	[Tooltip("Bounds, in global coordinates")]
	public Bounds bounds;

	[Tooltip("Point (in local coordinates) to test against bounds")]
	public Vector3 localPoint;

	public UnityEvent enteredBounds;
	public UnityEvent exitedBounds;

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	bool inBounds;

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Start() {
		inBounds = InBounds();
	}
	
	void Update() {
		if (InBounds() != inBounds) {
			inBounds = !inBounds;
			if (inBounds) {
				if (enteredBounds != null) enteredBounds.Invoke();
			} else {
				if (exitedBounds != null) exitedBounds.Invoke();
			}
		}
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	
	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods
	bool InBounds() {
		return bounds.Contains(transform.position);
	}
	#endregion
}
