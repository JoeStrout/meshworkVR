using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class AppFunctions : MonoBehaviour {
	#region Public Properties

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods

	public void OpenURL(string url) {
		Application.OpenURL(url);
	}

	public void CaptureScreenshot(string path) {
		if (string.IsNullOrEmpty(path)) path = "Screenshot.png";
		//Application.CaptureScreenshot(path);
		UnityEngine.ScreenCapture.CaptureScreenshot(path);
	}

	/*
	public void ExternalCall(string jsFunctionName) {
		Application.ExternalCall(jsFunctionName);
	}
	*/

	public void Quit() {
		Application.Quit();
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#endif		
	}
	
	public void ClearEventFocus() {
		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null, null);
	}
	
	public void ClearAllPrefs() {
		PlayerPrefs.DeleteAll();
	}

	public void HideMouseCursor() {
		Cursor.visible = false;
	}
	
	public void ShowMouseCursor() {
		Cursor.visible = false;
	}
	
	public void ShowMouseCursor(bool showIt) {
		Cursor.visible = showIt;
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	#endregion
}
