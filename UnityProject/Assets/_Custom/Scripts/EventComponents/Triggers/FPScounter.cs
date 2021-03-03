using UnityEngine;
using System.Collections;
using System;

public class FPScounter : MonoBehaviour {

	public  float updateInterval = 0.5F;
	public FloatEvent onChange;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	
	void Update() {

		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - send change event and start new interval
		if( timeleft <= 0.0 ) {
			onChange.Invoke(accum / frames);
			timeleft = updateInterval;
			accum = 0.0f;
			frames = 0;
		}

	}
}
