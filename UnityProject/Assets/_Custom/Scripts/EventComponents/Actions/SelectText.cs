/* SelectText

This component sets the Text property of a Text component
with a list of options, and data it can receive from an event.

In case you don't have a Text component handy, you can also
hook up the resulting text with another event.
*/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectText : MonoBehaviour {
	#region Public Properties
	public string[] options;
	
	public StringEvent textChanged;
	
	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	
	/// <summary>
	/// Set the text to one of our options, specified by index.
	/// If the index is out of bounds, sets a null string.
	/// </summary>
	/// <param name="index">index of option to select</param>
	public void SelectTextByIndex(int index) {
		if (index < 0 || index >= options.Length) ApplyText(null);
		else ApplyText(options[index]);
	}
	
	/// <summary>
	/// Set the text to options[1] if the parameter is true,
	/// or options[0] otherwise.
	/// </summary>
	/// <param name="pick1"></param>
	public void SelectTextByBool(bool pick1) {
		SelectTextByIndex(pick1 ? 1 : 0);
	}
		
	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods
	void ApplyText(string text) {
		Text textComp = GetComponent<Text>();
		if (textComp != null) textComp.text = text;
		
		if (textChanged != null) textChanged.Invoke(text);
	}
	#endregion
}
