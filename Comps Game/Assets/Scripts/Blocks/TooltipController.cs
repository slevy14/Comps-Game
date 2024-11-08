using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private string tooltip;
    [SerializeField] private float tooltipWait = 2f;
    [SerializeField] private GameObject tooltipPrefab;

    public void PrintTooltip() {
        Debug.Log(tooltip + " from " + this.gameObject.name);
    }

    public void HideTooltip() {
        Debug.Log("hiding tooltip from " + this.gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        // PrintTooltip();
    }

    public void OnPointerExit(PointerEventData eventData) {
        // HideTooltip();
    }
}
