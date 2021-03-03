using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TextOutput : MonoBehaviour {

	public int lineLimit = 60;

	TextMeshProUGUI outputText;
	List<string> lines;
	
	static TextOutput _defaultInstance;
	
	void InitIfNeeded() {
		if (_defaultInstance == null) _defaultInstance = this;
		if (outputText == null) {
			outputText = GetComponent<TextMeshProUGUI>();
			lines = new List<string>(outputText.text.Split(new char[] {'\n', '\r'}));			
		}
	}
	
	void Awake() {
		InitIfNeeded();
	}

	public void PrintLine(string line) {
		InitIfNeeded();
		foreach (string newLine in line.Split(new char[] {'\n', '\r'})) {
			lines.Add(newLine);
		}
		while (lines.Count > lineLimit) lines.RemoveAt(0);
		outputText.text = string.Join("\n", lines.ToArray());
	}
	
	public void Clear() {
		InitIfNeeded();
		lines.Clear();
		outputText.text = "";
	}
	
	public static void Print(string line) {
		_defaultInstance.PrintLine(line);
	}
	
}
