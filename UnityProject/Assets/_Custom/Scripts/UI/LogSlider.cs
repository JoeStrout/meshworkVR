/*
This script manages a slider that represents a logarithmic scale.
It's used for the transform scale slider (perhaps among other things).
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogSlider : MonoBehaviour
{
	public NumericField valueField;
	
	public float minActualValue = 0.01f;
	public float maxActualValue = 100f;
	public float initialValue = 1;
	
	public float actualValue {
		get {
			return _actualValue;
		}
		set {
			if (_actualValue == value) return;
			_actualValue = value;
			float log = Mathf.Log10(value);
			float minLog = Mathf.Log10(minActualValue);
			float maxLog = Mathf.Log10(maxActualValue);
			slider.value = Mathf.InverseLerp(minLog, maxLog, log);
		}		
	}
	float _actualValue;
	
	Slider slider;
	
	protected void Awake() {
		slider = GetComponent<Slider>();
		slider.onValueChanged.AddListener(NoteSliderChanged);
	}
	
	protected void Start() {
		actualValue = initialValue;
	}
	
	protected void OnDestroy() {
		slider.onValueChanged.RemoveAllListeners();
	}
	
	public void NoteSliderChanged(float newValue) {
		float minLog = Mathf.Log10(minActualValue);
		float maxLog = Mathf.Log10(maxActualValue);
		float log = Mathf.Lerp(minLog, maxLog, newValue);
		_actualValue = Mathf.Pow(10, log);
		valueField.value = _actualValue;
	}
}
