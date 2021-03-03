using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSteady : MonoBehaviour
{
	public Vector3 direction = Vector3.forward;
	public float speed = 1;
		
	void Update() {
		Vector3 lastPos = transform.position;
		transform.position += direction * speed * Time.deltaTime;
    }
}
