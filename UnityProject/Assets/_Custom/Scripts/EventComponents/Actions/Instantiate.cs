using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiate : MonoBehaviour
{
	public GameObject prefab;
	public Transform parent = null;
	public Transform matchTransformOf = null;
	
	public GameObjectEvent onInstantiate;
	
	[ContextMenu("Trigger")]
	public void InstantiatePrefab() {
		Transform t = transform;
		if (matchTransformOf != null) t = matchTransformOf;
		var go = Instantiate(prefab, t.position, t.rotation, parent);
		//Debug.Log("Instantiated " + go + " at " + t.position, go);
		
		// Stuff not appearing where you think it should?!
		// This can happen if this is invoked from LateUpdate (including via NetRelay),
		// if the script invoking it happens to fire before FinalIK does its thing.
		// In that case the avatar is not IK-posed yet, and its bones are in whatever
		// position the animator would have them in.  Very confusing!
		
		onInstantiate.Invoke(go);
	}
}
