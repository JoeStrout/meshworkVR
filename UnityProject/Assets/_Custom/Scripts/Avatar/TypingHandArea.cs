using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingHandArea : MonoBehaviour
{
	//public TMPro.TextMeshProUGUI debugText;
	/*	
	BoxCollider bounds;
	
	protected void Awake() {
		bounds = GetComponent<BoxCollider>();
	}
	
	protected void OnDisable() {
		var refs = GlobalRefs.instance;
		if (refs == null) return;
		for (int h=0; h<2; h++) {
			if (refs.typingHands[h] != null) refs.typingHands[h].SetActive(false);
			if (refs.normalHands[h] != null) refs.normalHands[h].SetActive(true);
		}
	}
	
	protected void Update() {
		var refs = XRReferences.instance;
		if (refs == null) return;
		//string s = "";
		for (int h=0; h<2; h++) {
			if (refs.typingHands[h] == null) continue;
			var inBounds = bounds.ContainsPoint(refs.typingHands[h].transform.position);
			//if (inBounds) {
			//	Vector3 v = refs.typingHands[h].transform.forward;
			//	//s += v.ToString() + "  ";
			//	if (Mathf.Abs(v.y) > 0.5f) inBounds = false;
			//}
			refs.typingHands[h].SetActive(inBounds);
			if (refs.normalHands[h] != null) refs.normalHands[h].SetActive(!inBounds);
		}
		//debugText.text = s;
	}
	*/
}
