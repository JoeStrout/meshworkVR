using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimParam : MonoBehaviour
{
	public Animator anim;
	public string parameterName;
	
	public void SetBoolParameter(bool value) {
		anim.SetBool(parameterName, value);
	}
	
	public void SetTrigger() {
		anim.SetTrigger(parameterName);
	}
	
	public void SetIntParameter(int value) {
		anim.SetInteger(parameterName, value);
	}

	public void SetFloatParameter(float value) {
		anim.SetFloat(parameterName, value);
	}
}
