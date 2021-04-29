using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class MicrophoneLoopback : MonoBehaviour
{
    

    GameObject micLoopback_GameObject;
    AudioSource micLoopback_AudioSource;
    [HideInInspector] public bool micLoopback_isMuted { get; private set; } // This var is exposed so other scripts may see if mic is muted w/o needing to expose AudioSource 
                                                                            // (use methods below to set mute status)

    [SerializeField] string micLoopbackAudioObjectName = "MicLoopback_AudioSource";
    [SerializeField] bool startMuted = false;
    [SerializeField] int maxWaitForAudioPermission = 60;

    [SerializeField] bool debug = true;

    int sampleRate, minFreq, maxFreq;
    string defaultMicName;

    int waitingForPermissionTimer = 0;


    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            StartMicLoopback();
        }
        else
        {
            Permission.RequestUserPermission(Permission.Microphone);
            InvokeRepeating("PermissionTest", 1, 1);
        }
#else
        StartMicLoopback();
#endif
    }


#if UNITY_ANDROID
    private void PermissionTest()
    {
        waitingForPermissionTimer++;
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone) || waitingForPermissionTimer > maxWaitForAudioPermission)
        {
            CancelInvoke("PermissionTest");
            StartMicLoopback();
        }
    }
#endif

    void StartMicLoopback()
    { 
        if (!(Microphone.devices.Length > 0))
        {
            Debug.LogError("No microphone detected; disabling MicrophoneLoopback.cs on: " + gameObject.name);
            return;
        }

        defaultMicName = Microphone.devices[0];


        Microphone.GetDeviceCaps(defaultMicName, out minFreq, out maxFreq);
        if (debug) Debug.Log(string.Format("GetDeviceCaps name=`{0}` minFreq=`{1}` maxFreq=`{2}`", defaultMicName, minFreq, maxFreq));


        micLoopback_GameObject = new GameObject();
        micLoopback_GameObject.name = micLoopbackAudioObjectName;
        micLoopback_GameObject.transform.SetParent(transform, false);

        micLoopback_AudioSource = micLoopback_GameObject.AddComponent<AudioSource>();

        micLoopback_isMuted = startMuted;
        micLoopback_AudioSource.mute = micLoopback_isMuted;
        micLoopback_AudioSource.loop = true;

        micLoopback_AudioSource.clip = Microphone.Start(defaultMicName, true, 10, maxFreq);
        micLoopback_AudioSource.clip.name = "Microphone Loopback 'Clip'";

        while (!(Microphone.GetPosition(null) > 0)) { } // wait 0 samples before audio starts to play (minimize inherent latency)

        micLoopback_AudioSource.Play();
    }

    


    //  PUBLIC METHODS TO UN-/MUTE MIC -----------------------------------
    //
    public void MicrophoneLoopback_Mute()
    {
        MicrophoneLoopback_Mute(true);
    }

    public void MicrophoneLoopback_Unmute()
    {
        MicrophoneLoopback_Mute(false);
    }

    public void MicrophoneLoopback_Mute(bool shouldMicBeMuted)
    {
        micLoopback_AudioSource.mute = shouldMicBeMuted;
        micLoopback_isMuted = shouldMicBeMuted;
    }
    //
    //--------------------------------------------------------------------
}
