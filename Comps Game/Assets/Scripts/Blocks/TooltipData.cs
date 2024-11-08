using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private string blockName;
    [SerializeField] private string tooltip;
    [SerializeField] private GameObject tooltipPrefab;

    public void PrintTooltip() {
        Debug.Log(tooltip + " from " + this.gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipController.Instance.StartTooltipTimer(blockName, tooltip);
        PrintTooltip();
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipController.Instance.StopTooltip();
        // HideTooltip();
    }
}
