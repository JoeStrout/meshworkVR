/*
Represents one of those little drumsticks used for pressing keyboard/keypad keys.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingStick : Tool
{
	public bool CanApply() {
		foreach (var area in TypingHandArea.instances) {
			if (area.collider.ContainsPoint(transform.position)) return true;
		}
		return false;
	}
}
