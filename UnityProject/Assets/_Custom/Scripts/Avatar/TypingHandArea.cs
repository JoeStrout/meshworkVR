/*
Represents a volume within which we want to display typing sticks.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingHandArea : MonoBehaviour
{
	public static List<TypingHandArea> instances = new List<TypingHandArea>();
	
	public BoxCollider collider { get; private set; }
	
	protected void Awake() {
		collider = GetComponent<BoxCollider>();
	}
	
	protected void OnEnable() {
		instances.Add(this);
	}
	
	protected void OnDisable() {
		instances.Remove(this);
	}
}
