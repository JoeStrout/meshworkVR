using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadScene : MonoBehaviour {
	[Tooltip("Invoked periodically while level loads; receives progress in range 0-1.")]
	public FloatEvent progress;

	[Tooltip("Invoked when the loading of the level is complete.")]
	public UnityEvent complete;

	/// <summary>
	/// Start loading the specified scene (asynchronously).
	/// </summary>
	/// <param name="sceneName">Scene name.</param>
	public void Load(string sceneName) {
		StartCoroutine(LoadAsync(sceneName));
	}

	/// <summary>
	/// Coroutine to load the scene asynchronously, invoking the
	/// appropriate callbacks.
	/// </summary>
	/// <param name="sceneName">Scene name.</param>
	IEnumerator LoadAsync(string sceneName) {
		AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		while (!async.isDone) {
			if (progress != null) progress.Invoke(async.progress);
			yield return null;
		}
		if (complete != null) complete.Invoke();
	}
}
