/*
This script manages a collection of VRPushButtons that together act as a virtual keyboard.
Just put it on the container object that contains a bunch of VRPushButtons.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class VRKeyboard : MonoBehaviour
{
	public bool applyToFocusedField;
	
	[Header("Sounds")]
	public AudioClip keyClick;
	public AudioClip keyClickSpace;
	public AudioClip keyClickReturn;
	public AudioClip keyClickBackspace;
	
	public AudioSource audioSrc;

	[Header("Events")]
	public StringEvent onCharKey;
	public UnityEvent onBackspace;
	public UnityEvent onClear;
	public UnityEvent onReturn;
	public StringEvent onSuggestion;
	
	public VRKeyboardKey[] suggestionKeys;

	List<VRKeyboardKey> keys = new List<VRKeyboardKey>();
	List<VRKeyboardKey> shiftKeys = new List<VRKeyboardKey>();

	bool shiftPressed = false;

	public void NoteKey(VRKeyboardKey key) {
		keys.Add(key);
		if (key.function == VRKeyboardKey.Function.Shift) shiftKeys.Add(key);
	}
		
	public void HandleCharKey(string c, VRKeyboardKey key) {
		PlaySound(c == " " ? keyClickSpace : keyClick, key);
		onCharKey.Invoke(c);
		if (applyToFocusedField) {
			TMP_InputField field = null;
			GameObject currentObj = EventSystem.current.currentSelectedGameObject;
			if (currentObj != null) field = currentObj.GetComponent<TMP_InputField>();
			if (field != null) field.ReplaceSelectedText(c);
		}
	}
	
	public void HandleBackspace(VRKeyboardKey key) {
		PlaySound(keyClickBackspace, key);
		onBackspace.Invoke();
		if (applyToFocusedField) {
			TMP_InputField field = null;
			GameObject currentObj = EventSystem.current.currentSelectedGameObject;
			if (currentObj != null) field = currentObj.GetComponent<TMP_InputField>();
			if (field != null) field.DoBackspace();
		}
	}
	
	public void HandleReturn(VRKeyboardKey key) {
		PlaySound(keyClickReturn, key);
		onReturn.Invoke();
		if (applyToFocusedField) {
			TMP_InputField field = null;
			GameObject currentObj = EventSystem.current.currentSelectedGameObject;
			if (currentObj != null) field = currentObj.GetComponent<TMP_InputField>();
			if (field != null) field.ReplaceSelectedText("\n");
		}
	}
	
	public void HandleClear(VRKeyboardKey key) {
		PlaySound(keyClickBackspace, key);
		onClear.Invoke();
		if (applyToFocusedField) {
			TMP_InputField field = null;
			GameObject currentObj = EventSystem.current.currentSelectedGameObject;
			if (currentObj != null) field = currentObj.GetComponent<TMP_InputField>();
			if (field != null) field.text = "";			
		}
	}
	
	public void HandleSuggestion(string suggestion, VRKeyboardKey key) {
		PlaySound(keyClick, key);
		onSuggestion.Invoke(suggestion);
	}
	
	public void SetSuggestions(List<string> suggestions) {
		for (int i=0; i<suggestionKeys.Length; i++) {
			if (i >= suggestions.Count) suggestionKeys[i].caption = "";
			else suggestionKeys[i].caption = suggestions[i];
		}
	}
	
	public void UpdateShiftState() {
		bool nowPressed = false;
		foreach (var key in shiftKeys) if (key.isPressed) nowPressed = true;
		if (nowPressed == shiftPressed) return;	// no change
		shiftPressed = nowPressed;
		foreach (var key in keys) key.NoteShiftState(shiftPressed);
	}
	
			
	void PlaySound(AudioClip sound, VRKeyboardKey key) {
		if (!audioSrc) return;
		if (sound == null) sound = keyClick;
		if (sound == null) return;
		audioSrc.clip = sound;
		if (key != null) audioSrc.transform.position = key.transform.position;
		audioSrc.Play();
	}
}
