using UnityEngine;
using System.Collections.Generic;

public static class GameObjectUtils {

	/// <summary>
	/// Sets the layer of this object and its child objects, recursively.
	/// You can use this to only update objects in a certain layer, or
	/// all objects (by specifying layerToChange = -1).  The default is
	/// to change layer 0, i.e., the Default layer.
	/// </summary>
	/// <param name="gameObject">Game object to update (along with its children)</param>
	/// <param name="newLayer">New layer to set.</param>
	/// <param name="layerToChange">Layer to change, or -1 to change any layer.</param>
	public static void SetLayerRecursively(this GameObject gameObject, int newLayer, int layerToChange=0) {
		if (layerToChange < 0 || gameObject.layer == layerToChange) {
			gameObject.layer = newLayer;
		}
		Transform t = gameObject.transform;
		for (int i = 0; i < t.childCount; i++) {
			t.GetChild(i).gameObject.SetLayerRecursively(newLayer);
		}
	}

	/// <summary>
	/// Return whether the second object is a child (or any descendant)
	/// of the first one in the transform hierarchy.
	/// </summary>
	/// <param name="gameObject">Game object.</param>
	/// <param name="potentialChild">Potential child.</param>
	public static bool Contains(this GameObject gameObject, GameObject potentialChild) {
		// Quickest way to check this is to walk up the ancestor chain of the potential child.
		Transform ourT = gameObject.transform;
		Transform t = potentialChild.transform.parent;
		while (t != null) {
			if (t == ourT) return true;
			t = t.parent;
		}
		return false;
	}

}
