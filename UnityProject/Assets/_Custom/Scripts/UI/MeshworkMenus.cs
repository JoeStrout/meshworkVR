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
	
	protected Menu selectionMenu { get {
		if (_selectionMenu == null) {
			_selectionMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadSelectionMenu(_selectionMenu);
		}
		return _selectionMenu;
	}}
	Menu _selectionMenu = null;
	
	protected Menu modeMenu { get {
		if (_modeMenu == null) {
			_modeMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadModeMenu(_modeMenu);
		}
		return _modeMenu;
	}}
	Menu _modeMenu = null;
	
	protected Menu createMenu { get {
		if (_createMenu == null) {
			_createMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadCreateMenu(_createMenu);
		}
		return _createMenu;
	}}
	Menu _createMenu = null;
	
	protected Menu modifyMenu { get {
		if (_modifyMenu == null) {
			_modifyMenu = Instantiate(menuPrefab, mainMenu.transform);
			LoadModifyMenu(_modifyMenu);
		}
		return _modifyMenu;
	}}
	Menu _modifyMenu = null;
	
	protected void Start() {
		PrepareMenu(mainMenu, "Main Menu Test", false);
		mainMenu.AddItem("Scene", true, (item,left) => { item.ShowSubmenu(sceneMenu); } );
		mainMenu.AddItem("Edit", true, (item,left) => { item.ShowSubmenu(editMenu); } );
		mainMenu.AddItem("Selection", true, (item,left) => { item.ShowSubmenu(selectionMenu); });
		mainMenu.AddItem("Create", true, (item,left) => { item.ShowSubmenu(createMenu); });
		mainMenu.AddItem("Modify", true, (item,left) => { item.ShowSubmenu(modifyMenu); });
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
		menu.AddItem("Mode", true, (item,left) => { item.ShowSubmenu(modeMenu); });
		menu.AddItem("Transform...");
		menu.AddItem("Select All");
		menu.AddItem("Deselect");
		menu.AddItem("Invert Selection");
		menu.AddItem("Hide");
		menu.AddItem("Unhide All");
		menu.AddItem("Invert Hidden");
	}
	
	protected void LoadModeMenu(Menu menu) {
		PrepareMenu(menu, "Mode");
		menu.AddItem("Objects");
		menu.AddItem("Faces");
		menu.AddItem("Edges");
		menu.AddItem("Points");		
	}
	
	protected void LoadCreateMenu(Menu menu) {
		PrepareMenu(menu, "Create");
		menu.AddItem("Box...");
		menu.AddItem("Cylinder...");
		menu.AddItem("Cone...");
		menu.AddItem("Sphere...");
	}
	
	
	protected void LoadModifyMenu(Menu menu) {
		PrepareMenu(menu, "Modify");
		menu.AddItem("Twist...");
		menu.AddItem("Spherify...");
		menu.AddItem("Symmetry...");
		menu.AddItem("Clean Up...");
	}
}
