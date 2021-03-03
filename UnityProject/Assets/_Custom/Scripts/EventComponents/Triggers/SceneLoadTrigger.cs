/* SceneLoadTrigger

This component fires an event on Start or Awake, optionally
doing this only if Application.LoadedLevelName matches some
pattern.

*/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoadTrigger : MonoBehaviour {
	#region Public Properties
	[Multiline]
	public string note = 
@"This component fires an event on Start or Awake,
optionally doing this only if 
Application.LoadedLevelName matches some pattern.";

	public enum MatchStyle {
		Exact,
		Contains,
		StartsWith,
		EndsWith,
		RegEx
	}

	[Tooltip("Optional: what the level name must match for the events to fire.")]
	public string levelNamePattern;
	[Tooltip("How the levelNamePattern must match the actual name.")]
	public MatchStyle matchStyle = MatchStyle.Exact;

	public UnityEvent awakeEvent;
	public UnityEvent startEvent;

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events

	void Awake() {
		if (Applies() && awakeEvent != null) awakeEvent.Invoke();
	}

	void Start() {
		if (Applies() && startEvent != null) startEvent.Invoke();
	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods

	bool Applies() {
		string name = SceneManager.GetActiveScene().name;
		if (string.IsNullOrEmpty(levelNamePattern)) return true;
		switch (matchStyle) {
		case MatchStyle.Exact:
			return name == levelNamePattern;
		case MatchStyle.Contains:
			return name.Contains(levelNamePattern);
		case MatchStyle.StartsWith:
			return name.StartsWith(levelNamePattern);
		case MatchStyle.EndsWith:
			return name.EndsWith(levelNamePattern);
		case MatchStyle.RegEx:
			var rgx = new System.Text.RegularExpressions.Regex(levelNamePattern);
			return rgx.IsMatch(name);
		}
		return false;
	}

	#endregion
}
