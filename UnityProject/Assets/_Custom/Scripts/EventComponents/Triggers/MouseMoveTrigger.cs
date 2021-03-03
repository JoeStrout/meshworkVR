/* MouseMoveTrigger
Place this on a UI element that you want to cause mouse move events.
For example, it might be an Image (even with 0% opacity) behind all
your other UI elements, to send mouse events for your game.
*/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MouseMoveTrigger: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	// Event invoked when the mouse moves over this UI element.
	public UnityEvent onMouseMove;
		
	// Called when the pointer enters our GUI component.
	// Start tracking the mouse
	public void OnPointerEnter( PointerEventData eventData ) {
		StartCoroutine("TrackPointer");
	}
	
	// Called when the pointer exits our GUI component.
	// Stop tracking the mouse
	public void OnPointerExit( PointerEventData eventData ) {
		StopCoroutine("TrackPointer");
	}
	
	IEnumerator TrackPointer() {
		Vector3 mousePos = Vector3.zero;
		while (Application.isPlaying) {
			if (Input.mousePosition != mousePos) {
				if (onMouseMove != null) onMouseMove.Invoke();
				mousePos = Input.mousePosition;
			}
			yield return 0;
		}
	}

}
