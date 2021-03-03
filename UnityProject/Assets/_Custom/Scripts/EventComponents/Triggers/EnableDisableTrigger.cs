using UnityEngine;
using UnityEngine.Events;

public class EnableDisableTrigger : MonoBehaviour {
	public UnityEvent onEnabled;
	public UnityEvent onDisabled;

	void OnEnable() {
		if (onEnabled != null) onEnabled.Invoke();
	}
	
	void OnDisable() {
		if (onDisabled != null) onDisabled.Invoke();
	}

}
