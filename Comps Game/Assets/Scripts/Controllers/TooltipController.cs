using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipController : MonoBehaviour {

    // object
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private GameObject tooltipParentCanvas;
    [SerializeField] private GameObject currentTooltipObject;

    // tooltip info
    private string blockNameText;
    private string tooltipText;

    // status
    [SerializeField] private bool isTooltipActive;
    [SerializeField] private bool isReadyForTooltip = true;

    // timer
    [SerializeField] private float tooltipTimer;
    [SerializeField] private float tooltipWait;
    [SerializeField] private bool isTimerGoing;


    // SINGLETON
    public static TooltipController Instance = null; 

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
    }

    void Awake() {
        CheckSingleton();
    }


    void Update() {
        if (isTimerGoing && isReadyForTooltip && !isTooltipActive) {
            tooltipTimer += Time.deltaTime;
            if (tooltipTimer >= tooltipWait) {
                ShowTooltip(blockNameText, tooltipText);
            }
        }

        if (isTooltipActive) {
            // update tooltip position to mouse position
            // currentTooltipObject.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
            currentTooltipObject.transform.position = Input.mousePosition;
        }
    }

    public void StartTooltipTimer(string blockName, string tooltip) {
        isTimerGoing = true;
        this.blockNameText = blockName;
        this.tooltipText = tooltip;
    }

    // FIXME: CHILDREN ARE WRONG!
    public void ShowTooltip(string blockName, string tooltip) {
        // instantiate tooltip
        currentTooltipObject = Instantiate(tooltipPrefab, Input.mousePosition, transform.rotation, tooltipParentCanvas.transform);
        Debug.Log("instantiating tooltip!");
        // set tooltip name
        currentTooltipObject.transform.GetChild(1).GetComponent<TMP_Text>().text = blockName;
        // set tooltip text
        currentTooltipObject.transform.GetChild(2).GetComponent<TMP_Text>().text = tooltip;
        // set isactive true
        isTooltipActive = true;
        // set isready false
        isReadyForTooltip = false;
        // reset timer
        isTimerGoing = false;
        tooltipTimer = 0f;
    }

    public void StopTooltip() {
        // reset timer
        isTimerGoing = false;
        tooltipTimer = 0f;

        if (isTooltipActive) {
            HideTooltip();
        }
    }

    public void HideTooltip() {
        Debug.Log("hiding tooltip!");
        // set tooltip active false
        isTooltipActive = false;
        // set is ready true
        isReadyForTooltip = true;
        // destroy tooltip object
        Destroy(currentTooltipObject);
        // set current to null
        currentTooltipObject = null;
    }


}
