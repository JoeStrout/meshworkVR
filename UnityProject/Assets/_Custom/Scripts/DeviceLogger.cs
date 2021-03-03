using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeviceLogger : MonoBehaviour
{
	public Transform head;
	public Transform leftHand;
	public Transform rightHand;
	
	public StringEvent onPrint;

	bool recording;
	float nextStartTime;
	float stopTime;
	StreamWriter file;
	
	int lastCountdownShown = -1;
	
	protected void Start() {
		nextStartTime = Time.time + 10;
		Print("Recording in 10...");
	}
	
	protected void LateUpdate() {
		if (recording) {
			// update recording
			if (Time.time > stopTime) {
				// stop recording
				file.Close();
				file = null;
				Print("Recording stopped.");
				nextStartTime = Time.time + 10;
				Print("Next recording in 10...");
				recording = false;
			} else {
				WriteDataToFile();
			}
		} else {
			if (Time.time > nextStartTime) {
				// start recording
				stopTime = Time.time + 5;
				string fname = System.DateTime.Now.ToString("HH-mm-ss") + ".csv";
				string path = Path.Combine(Application.persistentDataPath, fname);
				file = new StreamWriter(path);
				WriteHeadersToFile();
				Print("Recording to " + fname + "...");
				recording = true;
			} else {
				// update countdown
				int countDown = Mathf.FloorToInt(nextStartTime - Time.time);
				if (countDown != lastCountdownShown) {
					Print($"{countDown}...");
					lastCountdownShown = countDown;
				}
			}
		}
	}
	
	void WriteHeadersToFile() {
		WriteHeaders("Head");
		WriteHeaders("Left");
		WriteHeaders("Right");
		file.WriteLine("");
	}
	
	void WriteHeaders(string prefix) {
		file.Write($"{prefix}X,{prefix}Y,{prefix}Z,{prefix}Rx,{prefix}Ry,{prefix}Rz,{prefix}Rw,");
	}
	
	void WriteDataToFile() {
		WriteDataForTransform(head);
		WriteDataForTransform(leftHand);
		WriteDataForTransform(rightHand);
		file.WriteLine("");
	}
	
	void WriteDataForTransform(Transform t) {
		file.Write(t.position.x.ToString("0.###") + ",");
		file.Write(t.position.y.ToString("0.###") + ",");
		file.Write(t.position.z.ToString("0.###") + ",");
		file.Write(t.rotation.x.ToString("0.###") + ",");
		file.Write(t.rotation.y.ToString("0.###") + ",");
		file.Write(t.rotation.z.ToString("0.###") + ",");
		file.Write(t.rotation.w.ToString("0.###") + ",");		
	}
	
	void Print(string s) {
		onPrint.Invoke(s);
	}
}
