using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VR;

#pragma warning disable 162		// spurious "unreachable code detected" errors due to #if blocks

public class PlatformTrigger : MonoBehaviour {

	public enum Platform {
		UnityEditor,
		VR,
		Desktop,
		
		Windows,
		Mac,
		Linux,
		Android,
		iOS
	}

	public Platform platform;
	public Platform treatEditorAsAlso = Platform.UnityEditor;
	
	public UnityEvent onAwake;
	public UnityEvent onStart;
	
	protected void Awake() {
		if (PlatformApplies()) onAwake.Invoke();
	}

	protected void Start() {
		if (PlatformApplies()) onStart.Invoke();
	}
		
	public bool PlatformApplies() {
		switch (platform) {
		case Platform.UnityEditor:
			#if UNITY_EDITOR
			return true;
			#endif
			return false;
		case Platform.VR:
			#if UNITY_EDITOR
			return treatEditorAsAlso == Platform.VR;
			#endif
			return UnityEngine.XR.XRDevice.isPresent;
		case Platform.Desktop:
			#if UNITY_EDITOR
			return treatEditorAsAlso == Platform.Desktop;
			#endif
			#if UNITY_STANDALONE
			return true;
			#endif
			return false;
		case Platform.Windows:
			#if UNITY_EDITOR
			if (treatEditorAsAlso == Platform.Windows) return true;
			#endif
			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			return true;
			#endif
			return false;
		case Platform.Mac:
			#if UNITY_EDITOR
			if (treatEditorAsAlso == Platform.Mac) return true;
			#endif
			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return true;
			#endif
			return false;
		case Platform.Linux:
			#if UNITY_EDITOR
			if (treatEditorAsAlso == Platform.Linux) return true;
			#endif
			#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
			return true;
			#endif
			return false;
		case Platform.Android:
			#if UNITY_EDITOR
			if (treatEditorAsAlso == Platform.Android) return true;
			#endif
			#if UNITY_ANDROID
			return true;
			#endif
			return false;
		case Platform.iOS:
			#if UNITY_EDITOR
			if (treatEditorAsAlso == Platform.iOS) return true;
			#endif
			#if UNITY_IOS
			return true;
			#endif
			return false;
		
		}
		return false;
	}
	
}
