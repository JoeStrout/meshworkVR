/*
This script manages a row of a listbox, like the messages list in the mail UI.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListRow : MonoBehaviour
{
	public FormatText[] cells;
	
	public Color[] rowColors = new Color[] { new Color(0.9f, 0.9f, 0.95f), new Color(0.92f, 0.92f, 0.92f)};
	public Color selectedColor = Color.blue;
	
	new public object tag;	// whatever you want to store here!
	
	public bool selected {
		get { return _selected; }
		set { _selected = value; UpdateColor(); }
	}
	
	Image[] cellBackgrounds;	
	bool _selected = false;
	Color backgroundColor;
	
	public void SetCell(int column, string content) { cells[column].SetString(content); }
	public void SetCell(int column, float content) { cells[column].SetFloat(content); }
	public void SetCell(int column, int content) { cells[column].SetInt(content); }
	
	public void SetBackgroundColor(Color color) {
		backgroundColor = color;
		UpdateColor();
	}
	
	void UpdateColor() {
		if (cellBackgrounds == null) {
			cellBackgrounds = new Image[cells.Length];
			for (int i=0; i<cells.Length; i++) cellBackgrounds[i] = cells[i].GetComponentInParent<Image>();
		}
		for (int i=0; i<cells.Length; i++) {
			if (cellBackgrounds[i] != null) cellBackgrounds[i].color = (_selected ? selectedColor : backgroundColor);
		}
	}
	
	public void SetColorForRowNumber(int rowNum) {
		if (rowColors == null || rowColors.Length < 1) return;
		SetBackgroundColor(rowColors[rowNum % rowColors.Length]);
	}
	
	public void NoteClicked() {
		GetComponentInParent<Listbox>().HandleRowClick(this);
	}
}
