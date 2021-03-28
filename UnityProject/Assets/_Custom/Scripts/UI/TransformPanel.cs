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
	public TMP_InputField posXFld;
	public TMP_InputField posYFld;
	public TMP_InputField posZFld;
	
	[Space(10)]
	public TMP_InputField rotXFld;
	public TMP_InputField rotYFld;
	public TMP_InputField rotZFld;
	
	[Space(10)]
	public TMP_InputField scaleFld;

	protected void Awake() {
		ResetPosition(); ResetRotation(); ResetScale();
	}

	[ContextMenu("Reset Position")]
	public void ResetPosition() {
		posXFld.text = posYFld.text = posZFld.text = "0";
	}

	[ContextMenu("Reset Rotation")]
	public void ResetRotation() {
		rotXFld.text = rotYFld.text = rotZFld.text = "0";
	}

	[ContextMenu("Reset Scale")]
	public void ResetScale() {
		scaleFld.text = "1";
	}
}
