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
	
	public Menu container {
		get { return GetComponentInParent<Menu>(); }
	}

	TextMeshProUGUI tmPro;
	
	public void Configure(string text, bool hasSubmenu, Action action=null) {
		itemText.SetString(text);
		submenuIndicator.SetActive(hasSubmenu);
		this.action = action;
		gameObject.name = text + " (MenuItem)";
		if (!hasSubmenu && action == null) {
			// if this menu has no submenu or action, give it a disabled appearance
			var txt = itemText.GetComponent<TextMeshProUGUI>();
			txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 0.35f);
		}
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
	
	/// <summary>
	/// Show a submenu under this menu item.
	/// </summary>
	public void ShowSubmenu(Menu submenu) {
		var parentMenu = GetComponentInParent<Menu>();
		parentMenu.CloseSubmenus();
		
		Transform submenuT = submenu.transform;
		Transform parentT = parentMenu.transform;
		
		submenuT.SetParent(parentT);
		submenuT.localRotation = Quaternion.identity;
		Vector3 pos = Vector3.right * parentMenu.width;
		pos.y = transform.position.y - parentT.position.y - 0.02f;
		submenuT.localPosition = pos;
		submenu.gameObject.SetActive(true);
	}

	/// <summary>
	/// Close this menu and show a UI panel in its place.
	/// </summary>
	public void ShowPanel(Grabbable panel) {
		// Find a position to the right and slightly further away than this menu item.
		Vector3 pos = transform.position;
		Vector3 camPos = Camera.main.transform.position;
		Vector3 dpos = pos - camPos;
		dpos.y = 0;
		dpos = dpos.normalized * 0.2f;
		var topMenu = container.topLevelMenu;
		topMenu.Close();
		panel.TeleportTo(pos, camPos);
	}

}


}