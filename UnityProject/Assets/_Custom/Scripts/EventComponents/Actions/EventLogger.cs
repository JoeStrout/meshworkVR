using UnityEngine;
using System.Collections.Generic;

public class EventLogger : MonoBehaviour {
	public string intParamFormat = "{0}";
	
	public void Log(string msg) {
		Debug.Log(msg);
	}
	
	public void LogWithInt(int param) {
		Debug.Log(string.Format(intParamFormat, param));
	}
}
