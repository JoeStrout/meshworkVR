/*
This script controls one menu item in the Meshwork UI system.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Meshwork.UI {

	public class MenuItem : MonoBehaviour, IPointerClickHandler
{
	public delegate void Action(MenuItem source, bool hitWithLeft);
	
	public FormatText itemText;
	public GameObject submenuIndicator;
	public Action action;
	
	TextMeshProUGUI tmPro;
	
	public void Configure(string text, bool hasSubmenu, Action action=null) {
		itemText.SetString(text);
		submenuIndicator.SetActive(hasSubmenu);
		this.action = action;
		gameObject.name = text + " (MenuItem)";
	}
	
	public void OnPointerClick(PointerEventData p) {
		var evtData = p as UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceEventData;
		bool isLeft = (evtData != null && evtData.interactor == GlobalRefs.instance.leftRayInteractor);
		if (evtData == null && Input.GetKey(KeyCode.LeftShift)) isLeft = true;
		Debug.Log($"{gameObject.name} Clicked (left:{isLeft})");
		GetComponentInParent<Menu>().CloseSubmenus();
		if (action != null) action(this, isLeft);
	}
	
	public float PreferredWidth() {
		if (tmPro == null) tmPro = itemText.GetComponent<TextMeshProUGUI>();
		return tmPro.preferredWidth + 40;
	}
	
	public void ShowSubmenu(Menu submenu) {
		var parentMenu = GetComponentInParent<Menu>();
		parentMenu.CloseSubmenus();
		
		Transform submenuT = submenu.transform;
		Transform parentT = parentMenu.transform;
		
		submenuT.SetParent(parentT);
		submenuT.localRotation = Quaternion.identity;
		submenuT.localPosition = Vector3.right * parentMenu.width;
		submenu.gameObject.SetActive(true);
	}

}


}