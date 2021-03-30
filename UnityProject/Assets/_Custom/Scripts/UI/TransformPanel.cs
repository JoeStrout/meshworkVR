/*
This script manages a Transform panel, which lets the user set the position, rotation,
and scale of an object (or the scene).
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TransformPanel : MonoBehaviour
{
	public Transform target;
	
	public FormatText title;
	
	[Space(10)]
	public NumericField posXFld;
	public NumericField posYFld;
	public NumericField posZFld;
	
	[Space(10)]
	public NumericField rotXFld;
	public NumericField rotYFld;
	public NumericField rotZFld;
	
	[Space(10)]
	public NumericField scaleFld;

	bool internalChange;

	protected void OnEnable() {		
		if (target == null) {
			ResetPosition(); ResetRotation(); ResetScale();
			title.SetString("Transform");
		} else {
			LoadFieldsFromTransform();
		}
	}

	[ContextMenu("Reset Position")]
	public void ResetPosition() {
		posXFld.value = posYFld.value = posZFld.value = 0;
	}

	[ContextMenu("Reset Rotation")]
	public void ResetRotation() {
		rotXFld.value = rotYFld.value = rotZFld.value = 0;
	}

	[ContextMenu("Reset Scale")]
	public void ResetScale() {
		scaleFld.value = 1;
	}
	
	public void ApplyValuesFromFields() {
		if (target == null || internalChange) return;
		target.localPosition = new Vector3(posXFld.value, posYFld.value, posZFld.value);
		target.localEulerAngles = new Vector3(rotXFld.value, rotYFld.value, rotZFld.value);
		target.localScale = Vector3.one * scaleFld.value;
		Debug.Log($"Updated transform of {target} with scale {scaleFld.value} -> {target.localScale}");
	}
	
	public void LoadFieldsFromTransform() {
		internalChange = true;
		
		posXFld.value = target.localPosition.x;
		posYFld.value = target.localPosition.y;
		posZFld.value = target.localPosition.z;
		
		Vector3 rotation = target.localEulerAngles;
		rotXFld.value = rotation.x;
		rotYFld.value = rotation.y;
		rotZFld.value = rotation.z;
		
		scaleFld.value = target.localScale.x;

		title.SetString($"{target.name} Transform");
		internalChange = false;
	}
}
