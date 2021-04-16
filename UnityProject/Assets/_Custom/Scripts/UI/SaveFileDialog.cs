/*
This script manages a canvas configured to let the user enter a file name
to save a file to (or select an existing file to replace).
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using TMPro;
public class SaveFileDialog : MonoBehaviour
{
	public Listbox fileList;
	public TMP_InputField nameField;
	public Button saveButton;
	public Toggle replaceCheckbox;
	public string[] extensionsToShow = new string[] { ".glb", ".obj" };
	
	public string dirPath { get; private set; }
		
	public UnityEvent onCancel;
	public StringEvent onConfirm;
	
	protected void Start() {
		dirPath = Application.persistentDataPath;
		Reload();
	}
	
	protected void OnEnable() {
		fileList.onRowSelected.AddListener(NoteRowSelected);
		UpdateSaveButton();
	}
	
	protected void OnDisable() {
		fileList.onRowSelected.RemoveListener(NoteRowSelected);
	}
	
	public void NoteRowSelected(int row) {
		if (row >= 0) {
			string path = (string)fileList.rows[row].tag;
			string name = Path.GetFileName(path);
			nameField.text = name;
		}
	}
	
	public void SaveSelectedFile() {
		string path = Path.Combine(dirPath, nameField.text);
		onConfirm.Invoke(path);
	}
	
	public void Cancel() {
		gameObject.SetActive(false);
		onCancel.Invoke();
	}
	
	public void NoteFileNameChange(string newName) {
		bool replacing = false;
		string fullPath = Path.Combine(dirPath, newName);
		for (int i=0; i<fileList.rows.Count; i++) {
			string existingPath = (string)fileList.rows[i].tag;
			if (fullPath == existingPath) {
				replacing = true;
				break;
			}
		}
		replaceCheckbox.gameObject.SetActive(replacing);
		replaceCheckbox.isOn = false;
		UpdateSaveButton();
	}
	
	public void UpdateSaveButton() {
		saveButton.interactable = !string.IsNullOrEmpty(nameField.text)
			&& (!replaceCheckbox.gameObject.activeSelf || replaceCheckbox.isOn);
	}
	
	public void Reload() {
		fileList.DeleteAllRows();
		List<string> files = new List<string>(Directory.GetFiles(dirPath));
		files.AddRange(Directory.GetDirectories(dirPath));
		files.Sort();
		Debug.Log($"found {files.Count} files and directories in {dirPath}");
		foreach (string path in files) {
			string name = Path.GetFileName(path);
			if (name.StartsWith(".")) continue;
			if (extensionsToShow != null && extensionsToShow.Length > 0) {
				string ext = Path.GetExtension(name);
				if (System.Array.IndexOf(extensionsToShow, ext) < 0) continue;
			}
			fileList.AddRow();
			int row = fileList.rows.Count - 1;
			fileList.rows[row].tag = path;
			fileList.rows[row].SetCell(0, name);
		}
		UpdateSaveButton();
	}
	
}
