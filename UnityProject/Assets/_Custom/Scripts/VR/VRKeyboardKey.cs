/*
This script configures a VRPushButton to act as a keyboard key.  Place it inside
a container object with a VRKeyboard component.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VRKeyboardKey : MonoBehaviour
{
	
	public enum Function {
		CharKey,
		Shift,
		Return,
		Backspace,
		Clear,
		Suggestion
	}
	
	public Function function = Function.CharKey;
	
	public string unshiftedChar = "r";
	public string shiftedChar = "R";

	public bool isPressed { get { return button.isPressed; } }

	public string caption {
		get { return button.caption; }
		set { button.caption = value; }
	}

	VRKeyboard keyboard;
	VRPushButton button;
	TextMeshPro label;
	
	protected void OnValidate() {
		label = GetComponentInChildren<TextMeshPro>();
		if (function == Function.CharKey && label != null) label.text = unshiftedChar;
	}
	
	protected void Awake() {
		keyboard = GetComponentInParent<VRKeyboard>();
		button = GetComponent<VRPushButton>();
		label = button.GetComponentInChildren<TextMeshPro>();
		keyboard.NoteKey(this);
		button.onPressed.AddListener(HandlePress);
		
		switch (function) {
		case Function.CharKey:
			label.text = unshiftedChar;
			string upper = unshiftedChar.ToUpperInvariant();
			if (upper.Length > 0 && upper[0] >= '0' && upper[0] <= '9') upper = "Alpha" + upper;
			foreach (var value in System.Enum.GetValues(typeof(KeyCode))) {
				if (value.ToString() == upper) {
					button.keyboardKey = (KeyCode)value;
					break;
				}
			}
			break;
			
		case Function.Shift:
			button.keyboardKey = (transform.position.x < 0 ? KeyCode.LeftShift : KeyCode.RightShift);
			button.onButtonDown.AddListener(HandleKeyDown);
			button.onButtonUp.AddListener(HandleKeyUp);
			break;
			
		case Function.Return:
			button.keyboardKey = KeyCode.Return;
			break;
		
		case Function.Clear:
			button.keyboardKey = KeyCode.Clear;
			break;
		
		case Function.Backspace:
			button.keyboardKey = KeyCode.Backspace;
			break;
		}
		
	}	
	
	protected void OnDestroy() {
		button.onPressed.RemoveAllListeners();
		button.onButtonDown.RemoveAllListeners();
		button.onButtonUp.RemoveAllListeners();
	}
	
	[ContextMenu("Press")]
	void HandlePress() {
		switch (function) {
		case Function.CharKey:		keyboard.HandleCharKey(label.text, this);		break;
		case Function.Return:		keyboard.HandleReturn(this);					break;
		case Function.Backspace:	keyboard.HandleBackspace(this);					break;
		case Function.Suggestion:	keyboard.HandleSuggestion(label.text, this);	break;
		case Function.Clear:		keyboard.HandleClear(this);						break;
		}
	}
	
	void HandleKeyDown() {
		if (function == Function.Shift) keyboard.UpdateShiftState();
	}
	
	void HandleKeyUp() {
		if (function == Function.Shift) keyboard.UpdateShiftState();		
	}
	
	public void NoteShiftState(bool shiftPressed) {
		if (function != Function.CharKey) return;
		label.text = shiftPressed ? shiftedChar : unshiftedChar;
	}
}
