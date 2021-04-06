/*
This script manages a canvas configured to present a list of files, and let the
user select one to open or operate on.
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SelectFileDialog : MonoBehaviour
{
	public Listbox fileList;
	public Button openButton;
	
	public string dirPath { get; private set; }
	
	public System.Action<string> onFileSelected;
	
	public UnityEvent onCancel;
	public StringEvent onConfirm;
	
	protected void Start() {
		dirPath = Application.persistentDataPath;
		Reload();
		
		openButton.interactable = false;
	}
	
	protected void OnEnable() {
		fileList.onRowSelected.AddListener(NoteRowSelected);		
	}
	
	protected void OnDisable() {
		fileList.onRowSelected.RemoveListener(NoteRowSelected);
	}
	
	void NoteRowSelected(int row) {
		openButton.interactable = (row >= 0);
	}
	
	public void OpenSelectedFile() {
		string path = (string)fileList.rows[fileList.FirstSelected()].tag;
		onConfirm.Invoke(path);
	}
	
	public void Cancel() {
		gameObject.SetActive(false);
		onCancel.Invoke();
	}
	
	public void Reload() {
		fileList.DeleteAllRows();
		string[] files = Directory.GetFiles(dirPath);
		Debug.Log($"found {files.Length} files in {dirPath}");
		foreach (string path in files) {
			string name = Path.GetFileName(path);
			if (name.StartsWith(".")) continue;
			fileList.AddRow();
			int row = fileList.rows.Count - 1;
			fileList.rows[row].tag = path;
			fileList.rows[row].SetCell(0, name);
			if (name.EndsWith(".jpg") || name.EndsWith(".png")) {
				StartCoroutine(GetTexture(row));
			}
		}
	}
	
	IEnumerator GetTexture(int row) {
		string url = "file://" + fileList.rows[row].tag;
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError) {
			Debug.Log(www.error);
		} else {
			Texture tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
			fileList.rows[row].GetComponentInChildren<RawImage>().texture = tex;
		}
	}
}
