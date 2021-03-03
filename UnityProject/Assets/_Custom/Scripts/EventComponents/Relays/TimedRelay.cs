using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TimedRelay : MonoBehaviour {
	#region Public Properties
	public float delay = 1f;
	public UnityEvent relay;

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	List<float> fireTimes = new List<float>();

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Start() {
	
	}
	
	void Update() {
		while (fireTimes.Count > 0 && fireTimes[0] <= Time.time) {
			fireTimes.RemoveAt(0);
			relay.Invoke();
		}
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	public void Trigger() {
		float t = Time.time + delay;
		var index = fireTimes.BinarySearch(t);
		if (index < 0) index = ~index;
		fireTimes.Insert(index, t);
	}

	public void Clear() {
		fireTimes.Clear();
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	#endregion
}
