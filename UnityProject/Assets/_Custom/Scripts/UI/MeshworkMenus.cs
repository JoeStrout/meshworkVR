/*
This script creates and manages all the menus in the main Meshwork UI.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meshwork.UI;

public class MeshworkMenus : MonoBehaviour
{
	public Menu menuPrefab;
	
	public Menu mainMenu;
	
	protected Menu sceneMenu { get {
		if (_sceneMenu == null) {
			_sceneMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadSceneMenu(_sceneMenu);
		}
		return _sceneMenu;
	}}
	Menu _sceneMenu = null;
	
	protected Menu editMenu { get {
		if (_editMenu == null) {
			_editMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadEditMenu(_editMenu);
		}
		return _editMenu;
	}}
	Menu _editMenu = null;
	
	protected void Start() {
		PrepareMenu(mainMenu, "Main Menu Test", false);
		mainMenu.AddItem("Scene", true, (item,left) => { item.ShowSubmenu(sceneMenu); } );
		mainMenu.AddItem("Edit", true, (item,left) => { item.ShowSubmenu(editMenu); } );
		mainMenu.AddItem("Selection", true);
		mainMenu.AddItem("Create", true);
		mainMenu.AddItem("Modify", true);
		mainMenu.AddItem("Tools...", false);
	}
	
	void PrepareMenu(Menu menu, string title, bool showTitle=false) {
		menu.gameObject.name = title + " (Menu)";
		menu.titleText.SetString(title);
		menu.ClearItems();
		menu.ShowTitle(showTitle);
	}
	
	
	protected void LoadSceneMenu(Menu menu) {
		PrepareMenu(menu, "Scene");
		menu.AddItem("Transform...");
		menu.AddItem("Background...");
	}
	
	protected void LoadEditMenu(Menu menu) {
		PrepareMenu(menu, "Edit");
		menu.AddItem("Undo");
		menu.AddItem("Redo");
		menu.AddItem("Cut");
		menu.AddItem("Copy");
		menu.AddItem("Paste");
		menu.AddItem("Delete");		
	}
	
	protected void LoadSelectionMenu(Menu menu) {
		PrepareMenu(menu, "Selection");
		menu.AddItem("Mode", true);
		menu.AddItem("Transform...");
		menu.AddItem("Select All");
		menu.AddItem("Deselect");
		menu.AddItem("Invert Selection");
		menu.AddItem("Hide");
		menu.AddItem("Unhide All");
		menu.AddItem("Invert Hidden");
	}
}
