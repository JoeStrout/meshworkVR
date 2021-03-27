/*
A small collection of functions for working with TMP_InputField.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class InputFieldUtils
{
	public static void ReplaceSelectedText(this TMP_InputField field, string newText) {
		string s = field.text;
		int selStart = field.selectionAnchorPosition;
		int selEnd = field.selectionFocusPosition;
		if (selEnd < selStart) {
			selStart = field.selectionFocusPosition;
			selEnd = field.selectionAnchorPosition;
		}
		s = s.Substring(0, selStart) + newText + s.Substring(selEnd);
		field.text = s;
		field.caretPosition = selStart + newText.Length;
	}
	
	public static void DoBackspace(this TMP_InputField field) {
		string s = field.text;
		int selStart = field.selectionAnchorPosition;
		int selEnd = field.selectionFocusPosition;
		if (selEnd < selStart) {
			selStart = field.selectionFocusPosition;
			selEnd = field.selectionAnchorPosition;
		}
		if (selEnd > selStart) {
			s = s.Substring(0, selStart) + s.Substring(selEnd);
		} else if (selStart > 0) {
			s = s.Substring(0, selStart-1) + s.Substring(selStart);
			selStart = selStart - 1;
		}
		field.text = s;
		field.caretPosition = selStart;
	}
}
