using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColliderUtils
{
	public static bool ContainsPoint(this Collider c, Vector3 pt) {
		return Vector3.Distance(c.ClosestPoint(pt), pt) < 0.001f;
	}
}
