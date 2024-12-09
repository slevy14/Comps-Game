using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    // placed on all blocks
    // stores data to be displayed in the tooltip

    [SerializeField] private string blockName;
    [SerializeField] private string tooltip;

    // show and hide tooltip on mouse over

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipController.Instance.StartTooltipTimer(blockName, tooltip);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipController.Instance.StopTooltip();
    }

    // debug method to print data
    public void PrintTooltip() {
        Debug.Log(tooltip + " from " + this.gameObject.name);
    }
}
