using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumericOffset : MonoBehaviour {

	public float neutralValue = 0;
	public string prefsKey = "";
	
	public FloatEvent onOffsetChanged;		// gets the offset
	public FloatEvent onValueChanged;		// gets the combined value (neutral + offset)
	
	float curOffset = 0;
	
	void Start() {
		if (!string.IsNullOrEmpty(prefsKey)) {
			curOffset = PlayerPrefs.GetFloat(prefsKey, curOffset);
		}
		
		onOffsetChanged.Invoke(curOffset);
		onValueChanged.Invoke(neutralValue + curOffset);
	}
	
	public void SetOffset(float newOffset) {
		curOffset = newOffset;
		
		onOffsetChanged.Invoke(curOffset);
		onValueChanged.Invoke(neutralValue + curOffset);
		
		if (!string.IsNullOrEmpty(prefsKey)) {
			PlayerPrefs.SetFloat(prefsKey, curOffset);
		}		
	}
}
