/*
This script manages a numeric input field.  Ultimately it will even evaluate
simple arithmatic expressions.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumericField : MonoBehaviour
{
	public string format = "0.###";

	public float value {
		get { return _value; }
		set {
			//if (value == _value) return;
			_value = value;
			field.text = _value.ToString(format);
		}
	}
	float _value;

	TMP_InputField field;
	
	public FloatEvent onValueChanged;
	
	protected void Awake() {
		field = GetComponent<TMP_InputField>();
	}
	
	public void NoteStringValueChanged(string newValue) {
		if (float.TryParse(newValue, out _value)) {
			onValueChanged.Invoke(_value);
		}
	}
	
	public void Commit() {
		float prevValue = _value;
		if (float.TryParse(field.text, out _value)) {
			if (_value != prevValue) onValueChanged.Invoke(_value);
			field.text = _value.ToString(format);
		}		
	}
	
}
