using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSin : MonoBehaviour
{
	[System.Serializable]
	public struct SinWave {
		public float amplitude;
		public float period;
	}
	public SinWave[] sinWaves;
	
	Vector3 moveAxis = Vector3.up;
	
	Vector3 defaultPos;
	
	protected void Start() {
		defaultPos = transform.position;
	}
	
	void Update() {
		float t = Time.time;
		float x = 0;
		float twoPi = Mathf.PI * 2;
		for (int i=0; i<sinWaves.Length; i++) {
			x += Mathf.Sin(t / sinWaves[i].period * twoPi) * sinWaves[i].amplitude;			
		}
		transform.position = defaultPos + moveAxis * x;
	}
}
