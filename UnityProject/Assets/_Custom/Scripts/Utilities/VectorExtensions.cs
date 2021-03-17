using System;
using UnityEngine;
using System.Collections.Generic;

public static class VectorExtensions {
	#region Vector3
	
	/// <summary>
	/// Returns the signed angle between this 2D vector and another.
	/// (This is unlike Vector2.Angle, which always returns the
	/// absolute value of the angle.)
	/// </summary>
	/// <returns>The signed angle, in degrees, from A to B.</returns>
	/// <param name="a">Vector this was called on.</param>
	/// <param name="b">Vector to measure the angle to.</param>
	public static float SignedAngleTo(this Vector2 a, Vector2 b) {
		return Mathf.Atan2( a.x*b.y - a.y*b.x, a.x*b.x + a.y*b.y ) * Mathf.Rad2Deg;
	}
	
	/// <summary>
	/// Returns the signed angle between this vector and the +X axis.
	/// </summary>
	/// <returns>The signed angle, reprenting the direction of this in degrees.</returns>
	/// <param name="a">Vector this was called on.</param>
	public static float SignedAngle(this Vector2 a) {
		return Mathf.Atan2( a.y, a.x ) * Mathf.Rad2Deg;
	}
	
	public static bool IsNaN(this Vector2 v) {
		return float.IsNaN(v.x) || float.IsNaN(v.y);
	}
	
	public static bool IsNaN(this Vector3 v) {
		return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
	}
	
	/// <summary>
	/// Returns the signed angle between this 3D vector and another,
	/// with respect to some orthogonal "up" vector.  If looking in
	/// the "up" direction, then + angles are counter-clockwise.
	/// </summary>
	/// <returns>The signed angle, in degrees, from A to B.</returns>
	/// <param name="a">Vector this was called on.</param>
	/// <param name="b">Vector to measure the angle to.</param>
	public static float SignedAngleTo(this Vector3 a, Vector3 b, Vector3 up) {
		return Mathf.Atan2(
			Vector3.Dot(up.normalized, Vector3.Cross(a, b)),
			Vector3.Dot(a, b)) * Mathf.Rad2Deg;
	}
	
	/// <summary>
	/// Find the mean value of a list of Vector3s.
	/// </summary>
	/// <param name="list"></param>
	/// <returns></returns>
	public static Vector3 Average(this List<Vector3> list) {
		Vector3 sum = Vector3.zero;
		for (int i=0; i<list.Count; i++) sum += list[i];
		return sum / list.Count;
	}
	
	/// <summary>
	/// Get the X and Z components of this vector into a Vector2 (as XY, of course).
	/// </summary>
	public static Vector2 XZ(this Vector3 v) {
		return new Vector2(v.x, v.z);
	}
	
	public static Vector3 WithX(this Vector3 v, float x) {
		return new Vector3(x, v.y, v.z);
	}
	
	public static Vector3 WithY(this Vector3 v, float y) {
		return new Vector3(v.x, y, v.z);
	}
	
	public static Vector3 WithZ(this Vector3 v, float z) {
		return new Vector3(v.x, v.y, z);
	}
	
	public static bool ApproximatelyEqual(this Vector3 v, Vector3 w) {
		return Mathf.Approximately(v.x, w.x)
			&& Mathf.Approximately(v.y, w.y)
			&& Mathf.Approximately(v.z, w.z);
	}
	
	#endregion
	
	#region Vector2
	
	/// <summary>
	/// Find the index of the closest point to this one in an array of points.
	/// </summary>
	/// <param name="pt">this point (point of interest)</param>
	/// <param name="pointArray">array of points</param>
	/// <returns>index of closest point, or -1 if pointArray is empty</returns>
	public static int IndexOfNearestIn(this Vector2 pt, Vector2[] pointArray) {
		float bestDsqr = 0;
		int bestIdx = -1;
		for (int i=0; i<pointArray.Length; i++) {
			float dsqr = (pointArray[i] - pt).sqrMagnitude;
			if (dsqr < bestDsqr || bestIdx < 0) {
				bestIdx = i;
				bestDsqr = dsqr;
			}
		}
		return bestIdx;
	}

	public static Vector2 WithX(this Vector2 v, float x) {
		return new Vector2(x, v.y);
	}
	
	public static Vector2 WithY(this Vector2 v, float y) {
		return new Vector2(v.x, y);
	}
	

	#endregion
	
	public static void RunUnitTests() {
		float ang = SignedAngleTo(new Vector3(1, 0, 0), new Vector3(0, 0, 1), Vector3.up);
		Check(ang, -90);
		ang = SignedAngleTo(new Vector3(1, 0, 0), new Vector3(0, 0, 1), Vector3.down);
		Check(ang, 90);
		
		ang = SignedAngleTo(new Vector3(1, 0, 1), new Vector3(1, 0, 0), Vector3.up);
		Check(ang, 45);
		ang = SignedAngleTo(new Vector3(5, 0, 5), new Vector3(1, 0, 0), 42*Vector3.down);
		Check(ang, -45);
		
		ang = SignedAngleTo(new Vector3(1, 0, 1), new Vector3(1, 0, 1), Vector3.up);
		Check(ang, 0);
		ang = SignedAngleTo(new Vector3(1, 0, 1), new Vector3(3, 0, 3), Vector3.down);
		Check(ang, 0);
		
		
		ang = SignedAngleTo(new Vector3(1, 0, 0), new Vector3(0, 1, 0), 17*Vector3.back);
		Check(ang, -90);
		ang = SignedAngleTo(new Vector3(1, 0, 0), new Vector3(0, 1, 0), Vector3.forward);
		Check(ang, 90);
		
		ang = SignedAngleTo(new Vector3(1, 1, 0), new Vector3(1, 0, 0), Vector3.back);
		Check(ang, 45);
		ang = SignedAngleTo(new Vector3(5, 5, 0), new Vector3(1, 0, 0), Vector3.forward);
		Check(ang, -45);
		
		ang = SignedAngleTo(new Vector3(1, 1, 0), new Vector3(1, 1, 0), Vector3.back);
		Check(ang, 0);
		ang = SignedAngleTo(new Vector3(1, 1, 0), new Vector3(3, 3, 0), 36*Vector3.forward);
		Check(ang, 0);
		
		
		ang = SignedAngleTo(new Vector3(0, 0, 7), new Vector3(0, 1, 0), Vector3.right);
		Check(ang, -90);
		ang = SignedAngleTo(new Vector3(0, 0, 1), new Vector3(0, 1, 0), Vector3.left);
		Check(ang, 90);
		
		ang = SignedAngleTo(new Vector3(0, 1, 1), new Vector3(0, 0, 1), Vector3.right);
		Check(ang, 45);
		ang = SignedAngleTo(new Vector3(0, 1, 1), new Vector3(0, 0, 4), Vector3.left);
		Check(ang, -45);
		
		ang = SignedAngleTo(new Vector3(0, 1, 1), new Vector3(0, 1, 1), Vector3.right);
		Check(ang, 0);
		ang = SignedAngleTo(new Vector3(0, 6, 6), new Vector3(0, 1, 1), Vector3.left);
		Check(ang, 0);
		
		ang = new Vector3(.2f, .9f, .3f).SignedAngleTo(new Vector3(.2f, .9f, .3f), new Vector3(-115.9f, 0f, 87.5f));
		Check(ang, 0);
	}
	
	static void Check(float result, float expected) {
		if (!Mathf.Approximately(result, expected)) {
			Debug.LogError("Unit test failure: got " + result + " where " +expected + " was expected");
		}
	}
	
}
