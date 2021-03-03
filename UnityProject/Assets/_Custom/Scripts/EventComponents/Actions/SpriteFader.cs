using UnityEngine;
using System.Collections.Generic;

public class SpriteFader : MonoBehaviour {
	#region Public Properties
	public bool applyToChildren = true;

	#endregion
	//--------------------------------------------------------------------------------
	#region Private Properties
	enum Mode {
		FadingIn,
		FadingOut,
		Done
	}

	float startTime;
	float duration;
	Mode mode;

	#endregion
	//--------------------------------------------------------------------------------
	#region MonoBehaviour Events
	void Update() {
		if (mode == Mode.Done) return;

		if (mode == Mode.FadingOut) {
			float t = (Time.time - startTime) / duration;
			if (t >= 1) {
				SetAlpha(0);
				mode = Mode.Done;
			} else {
				SetAlpha(1 - t);
			}
		}

		if (mode == Mode.FadingIn) {
			float t = (Time.time - startTime) / duration;
			if (t >= 1) {
				SetAlpha(1);
				mode = Mode.Done;
			} else {
				SetAlpha(t);
			}
		}

	}

	#endregion
	//--------------------------------------------------------------------------------
	#region Public Methods
	public void FadeOut(float fadeTime) {
		startTime = Time.time;
		duration = fadeTime;
		mode = Mode.FadingOut;
	}

	public void FadeIn(float fadeTime) {
		startTime = Time.time;
		duration = fadeTime;
		mode = Mode.FadingIn;
	}
	
	public void SetAlpha(float alpha) {
		if (applyToChildren) {
			foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
				SetOneAlpha(sr, alpha);
			}
		} else {
			SetOneAlpha(GetComponent<SpriteRenderer>(), alpha);
		}
	}
	#endregion
	//--------------------------------------------------------------------------------
	#region Private Methods
	void SetOneAlpha(SpriteRenderer sr, float alpha) {
		Color c = sr.color;
		c.a = alpha;
		sr.color = c;
	}
	#endregion
}
