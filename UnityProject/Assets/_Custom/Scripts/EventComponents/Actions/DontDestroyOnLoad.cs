using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
	public bool applyToThis = true;
	
	void Start() {
		if (applyToThis) DontDestroy(gameObject);
	}

	public void DontDestroy(GameObject obj) {
		if (obj.transform.parent != null) obj.transform.SetParent(null, true);
		DontDestroyOnLoad(obj);
	}

}
