using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TrueOrFalseRelay : MonoBehaviour {
	#region Public Properties
	public UnityEvent invokeIfTrue;
	public UnityEvent invokeIfFalse;

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	public void InvokeTrueOrFalse(bool condition) {
		// Careful!  Testing shows that if you set a Toggle's value from code, at
		// least during Awake and Start, it may invoke its OnValueChanged event
		// with the OLD value of the toggle, rather than the new value.  Doh.
		//Debug.Log("TrueOrFalseRelay invoked with " + condition, gameObject);
		
		if (condition) invokeIfTrue.Invoke();
		else invokeIfFalse.Invoke();
	}

	#endregion
}
