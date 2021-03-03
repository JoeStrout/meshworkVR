using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class FieldValueRelay : MonoBehaviour {
	
	public InputField field;
	
	public StringEvent onInvokeWithText;
	public FloatEvent onInvokeWithFloat;
	public IntEvent onInvokeWithInt;
	
	public void Invoke() {
		if (field == null) field = GetComponent<InputField>();
		if (field == null) return;
		
		string text = field.text;
		onInvokeWithText.Invoke(text);
		
		if (string.IsNullOrEmpty(text.Trim())) text = "0";
		
		float floatVal;
		if (float.TryParse(text, out floatVal)) onInvokeWithFloat.Invoke(floatVal);
		
		int intVal;
		if (int.TryParse(text, out intVal)) onInvokeWithInt.Invoke(intVal);
		
	}
}
