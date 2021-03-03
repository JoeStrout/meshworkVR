using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyTextAsset : MonoBehaviour
{
	public TextAsset textAsset;
	
	public bool applyOnStart = true;
	public StringEvent onApply;
	
	void Start() {
		if (applyOnStart) Apply();
	}

	void Apply() {
		onApply.Invoke(textAsset.text);
	}
}
