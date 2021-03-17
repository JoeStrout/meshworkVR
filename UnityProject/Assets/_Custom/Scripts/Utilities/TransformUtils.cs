using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtils
{
	/// <summary>
	/// Find all objects under this (directly or indirectly) with the given name.
	/// </summary>
	public static List<Transform> FindDescendentsNamed(this Transform t, string name) {
		List<Transform> result = new List<Transform>();
		t.AddDescendentsNamed(name, result);
		return result;
	}

	/// <summary>
	/// Find all objects under this (directly or indirectly) with the given name,
	/// and add them to the given list.
	/// </summary>
	public static void AddDescendentsNamed(this Transform t, string name, List<Transform> result) {
		for (int i=0; i<t.childCount; i++) {
			Transform kid = t.GetChild(i);
			if (kid.gameObject.name == name) result.Add(kid);
			if (kid.childCount > 0) kid.AddDescendentsNamed(name, result);
		}
	}

	/// <summary>
	/// Find a single object under this (directly or indirectly) with the given name.
	/// </summary>
	public static Transform FindDescendantNamed(this Transform t, string name) {
		for (int i=0; i<t.childCount; i++) {
			Transform kid = t.GetChild(i);
			if (kid.gameObject.name == name) return kid;
			if (kid.childCount > 0) {
				Transform result = kid.FindDescendantNamed(name);
				if (result != null) return result;
			}
		}
		return null;
	}
	
	/// <summary>
	/// Make this transform face the main camera.
	/// </summary>
	/// <param name="t"></param>
	public static void FaceCamera(this Transform t) {
		t.rotation = Quaternion.LookRotation(t.position - Camera.main.transform.position);
	}
	
	/// <summary>
	/// Calculate the bounding rect of the given RectTransform within the canvas.
	/// You'd think this would be something easy and built in, but it's not.
	/// I'm not certain this code works in all cases, but it at least seems to work
	/// for a RectTransform directly under a Canvas set up for top-left origin.
	/// </summary>
	public static Rect BoundingRect(this RectTransform rt) {
		Vector2 canvasSize = (rt.GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
		Vector2 min = rt.anchorMin * canvasSize + rt.offsetMin;
		Vector2 max = rt.anchorMax * canvasSize + rt.offsetMax;
		return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
		
	}

	public static RectTransform RectTransform(this Component c) {
		return c.transform as RectTransform;
	}
	
	public static RectTransform RectTransform(this GameObject gob) {
		return gob.transform as RectTransform;
	}
	
}
