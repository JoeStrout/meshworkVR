using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class MicrophoneLoopback : MonoBehaviour {
	GameObject micLoopback_GameObject;
	AudioSource micLoopback_AudioSource;
	[SerializeField] string micLoopbackAudioObjectName = "MicLoopback_AudioSource";
	[SerializeField] bool autoStart = false;
	[SerializeField] bool debug = true;
	
	int sampleRate, minFreq, maxFreq;
	string defaultMicName;
	bool permissionGranted = false;
	bool permissionDeniedDontAskAgain = false;
	
	public enum State {
		Off,				// user doesn't want loopback functionality
		Starting,			// setting up and waiting for mic permission
		On					// actively passing audio from mic to speaker
	}
	
	public BoolEvent onChange;
	public UnityEvent onLoopbackStart;
	public UnityEvent onLoopbackStop;
	
	public State state { get; private set;}
		
	void Start() {
		if (autoStart) EnterState(State.Starting);
		else onLoopbackStop.Invoke();
	}
	
	void EnterState(State newState) {
		switch (newState) {
		case State.Starting:
			if (permissionDeniedDontAskAgain) EnterState(State.Off);
			else if (!permissionGranted) RequestPermission();
			break;
			
		case State.On:
			onLoopbackStart.Invoke();
			onChange.Invoke(true);
			if (debug) Debug.Log("Microphone loopback started");
			break;

		case State.Off:
			if (micLoopback_AudioSource != null && micLoopback_AudioSource.isPlaying) {
				micLoopback_AudioSource.Stop();
				Microphone.End(null);
			}
			onLoopbackStop.Invoke();
			onChange.Invoke(false);
			if (debug) Debug.Log("Microphone loopback stopped");
			break;
		}
		state = newState;
	}
	
	protected void Update() {
		switch (state) {
		case State.Starting:
			// Check permissions; when we get it, prepare the mic to actually do its thing
			if (Permission.HasUserAuthorizedPermission(Permission.Microphone)) BeginMic();
			break;
		}
	}
	
	void RequestPermission() {
		#if UNITY_ANDROID
		// Dang.  Looks like the permission callbacks are only available in 2020.  :(
		/*
		var callbacks = new PermissionCallbacks();
		callbacks.PermissionDenied += (s) => {
			permissionGranted = false;
			permissionDeniedDontAskAgain = false;
		};
		callbacks.PermissionGranted += (s) => {
			permissionGranted = true;
			permissionDeniedDontAskAgain = false;
		};
		callbacks.PermissionDeniedAndDontAskAgain += (s) =>  {
			permissionGranted = false;
			permissionDeniedDontAskAgain = true;
		};			
		Permission.RequestUserPermission(Permission.Microphone, callbacks);
		*/
		// So we'll have to just ask once, whenever told to start.
		Permission.RequestUserPermission(Permission.Microphone);
		#else
		permissionGranted = true;
		permissionDeniedDontAskAgain = false;
		#endif
	}
	
	void BeginMic() {
		if (Microphone.devices.Length == 0) {
			Debug.LogError("No microphone detected; disabling MicrophoneLoopback.cs on: " + gameObject.name);
			EnterState(State.Off);
			this.enabled = false;
			return;
		}
		
		defaultMicName = Microphone.devices[0];
		Microphone.GetDeviceCaps(defaultMicName, out minFreq, out maxFreq);
		if (debug) Debug.Log(string.Format("GetDeviceCaps name=`{0}` minFreq=`{1}` maxFreq=`{2}`", defaultMicName, minFreq, maxFreq));
		
		if (micLoopback_GameObject == null) {
			micLoopback_GameObject = new GameObject();
			micLoopback_GameObject.name = micLoopbackAudioObjectName;
			micLoopback_GameObject.transform.SetParent(transform, false);
			
			micLoopback_AudioSource = micLoopback_GameObject.AddComponent<AudioSource>();
		}
		
		micLoopback_AudioSource.mute = false;
		micLoopback_AudioSource.loop = true;
		
		micLoopback_AudioSource.clip = Microphone.Start(null, true, 10, maxFreq);
		micLoopback_AudioSource.clip.name = "Microphone Loopback 'Clip'";
		
		while (Microphone.GetPosition(null) == 0) {} // wait 0 samples before audio starts to play (minimize inherent latency)
		
		micLoopback_AudioSource.Play();
		EnterState(State.On);
	}
	
	public void EnableMic(bool enableIt) {
		if (enableIt) {
			if (state == State.Off) EnterState(State.Starting);
		} else {
			if (state != State.Off) EnterState(State.Off);
		}
	}
}
