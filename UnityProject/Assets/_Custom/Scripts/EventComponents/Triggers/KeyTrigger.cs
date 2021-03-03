using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class KeyTrigger : MonoBehaviour {
	#region Public Properties
	[System.Serializable]
	public class Element {
		[Tooltip("Key this event is triggered by.")]
		public KeyCode keyCode;
		[Tooltip("Event to fire when the key is pressed.")]	// hmm, doesn't seem to appear!
		public UnityEvent keyDownEvent;
		[Tooltip("Event to fire when the key is released.")]
		public UnityEvent keyUpEvent;
	};

	[Tooltip("List of key/event combinations.")]
	public List<Element> elements = new List<Element>(1);

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Update() {
		foreach (var elem in elements) {
			if (Input.GetKeyDown(elem.keyCode)) elem.keyDownEvent.Invoke();
			if (Input.GetKeyUp(elem.keyCode)) elem.keyUpEvent.Invoke();
		}
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	
	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	#endregion
}
