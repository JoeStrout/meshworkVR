/*
This script manages a UI Listbox, composed of zero or more ListRows.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Listbox : MonoBehaviour
{
	public ListRow listRowPrototype;
	
	public float minHeight = 100;
	
	public List<FormatText> columnHeaders;
	
	[HideInInspector]
	public List<ListRow> rows;
	
	float rowHeight;
	
	public IntEvent onRowSelected;
	
	protected void Awake() {
		listRowPrototype.gameObject.SetActive(false);
		rowHeight = (listRowPrototype.transform as RectTransform).sizeDelta.y;
		rows = new List<ListRow>();
	}
	
	[ContextMenu("Add Row")]
	public ListRow AddRow() {
		var noob = Instantiate(listRowPrototype, listRowPrototype.transform.parent) as ListRow;
		var rt = noob.transform as RectTransform;
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -rowHeight * rows.Count);
		rows.Add(noob);
		noob.gameObject.SetActive(true);
		noob.SetColorForRowNumber(rows.Count);
		AdjustSize();
		return noob;
	}
	
	[ContextMenu("Delete All Rows")]
	public void DeleteAllRows() {
		foreach (var row in rows) Destroy(row.gameObject);
		rows.Clear();
		AdjustSize();
	}
	

	public void DeleteRow(int row) {
		Destroy(rows[row].gameObject);
		rows.RemoveAt(row);
		for (int i=row; i<rows.Count; i++) {
			var rt = rows[i].transform as RectTransform;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -rowHeight * i);
		}
		onRowSelected.Invoke(-1);
	}
	
	public void SelectRow(int row, bool deselectOthers=true) {
		rows[row].selected = true;
		if (deselectOthers) {
			for (int i=0; i<rows.Count; i++) {
				if (i != row && rows[i].selected) rows[i].selected = false;
			}
		}
		onRowSelected.Invoke(row);
	}
	
	public void HandleRowClick(ListRow row) {
		for (int i=0; i<rows.Count; i++) if (rows[i] == row) {
			SelectRow(i);
			break;
		}
	}

	[ContextMenu("Select None")]
	public void SelectNone() {
		for (int i=0; i<rows.Count; i++) {
			if (rows[i].selected) rows[i].selected = false;
		}
	}
	
	[ContextMenu("Select Next")]
	public void SelectNext() {
		int i = FirstSelected() + 1;
		if (i >= rows.Count) i = 0;
		if (i < rows.Count) SelectRow(i);
	}
	
	public int FirstSelected() {
		for (int i=0; i<rows.Count; i++) {
			if (rows[i].selected) return i;
		}
		return -1;
	}

	void AdjustSize() {
		float height = rowHeight * rows.Count;
		if (height < minHeight) height = minHeight;
		(transform as RectTransform).sizeDelta = new Vector3(0, height);
	}
	
}
