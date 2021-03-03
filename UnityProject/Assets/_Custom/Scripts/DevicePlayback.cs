using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevicePlayback : MonoBehaviour
{
	public Transform head;
	public Transform leftHand;
	public Transform rightHand;

	public TextAsset recording;
	
	string[] lines;
	int nextLineNum = 0;
	
	protected void Start() {
		Application.targetFrameRate = 60;
		
		lines = recording.text.Split(new char[] {'\n'});
		Debug.Log($"Loaded recording with {lines.Length} lines");
	}
	
	protected void Update() {
		if (lines.Length <= 0) return;
		if (nextLineNum >= lines.Length) nextLineNum = 0;
		string[] fields = lines[nextLineNum].Split(new char[]{','});
		if (fields.Length >= 21) {
			UpdateTransform(head, fields, 0);
			UpdateTransform(leftHand, fields, 7);
			UpdateTransform(rightHand, fields, 14);
		}
		nextLineNum++;
	}
	
	public void UpdateTransform(Transform t, string[] fields, int idx) {
		Vector3 p = new Vector3();
		float.TryParse(fields[idx], out p.x);
		float.TryParse(fields[idx+1], out p.y);
		float.TryParse(fields[idx+2], out p.z);
		t.position = p;
		Quaternion q = new Quaternion();
		float.TryParse(fields[idx+3], out q.x);
		float.TryParse(fields[idx+4], out q.y);
		float.TryParse(fields[idx+5], out q.z);
		float.TryParse(fields[idx+6], out q.w);
		t.rotation = q;
	}
}
