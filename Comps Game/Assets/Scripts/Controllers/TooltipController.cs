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
            // default to bottom right of mouse
            Vector3 tooltipPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y - currentTooltipObject.GetComponent<RectTransform>().rect.height, Input.mousePosition.z);
            // flip to left side of mouse if out of bounds horizontally
            if (currentTooltipObject.GetComponent<RectTransform>().rect.width + Input.mousePosition.x >= 1960) {
                tooltipPos.x -= currentTooltipObject.GetComponent<RectTransform>().rect.width;
            }
            // flip to above mouse if out of bounds vertically
            if ((1080 - Input.mousePosition.y) + currentTooltipObject.GetComponent<RectTransform>().rect.height >= 1120) {
                tooltipPos.y += currentTooltipObject.GetComponent<RectTransform>().rect.height;
            }

            currentTooltipObject.transform.position = tooltipPos;
        }
    }

    public void StartTooltipTimer(string blockName, string tooltip) {
        // start timer for tooltip display
        // update with data to be displayed
        isTimerGoing = true;
        this.blockNameText = blockName;
        this.tooltipText = tooltip;
    }

    // if something breaks with this
    // it's probably that the children are wrong in the hierarchy
    public void ShowTooltip(string blockName, string tooltip) {
        // instantiate tooltip
        currentTooltipObject = Instantiate(tooltipPrefab, Input.mousePosition, transform.rotation, tooltipParentCanvas.transform);
        Debug.Log("instantiating tooltip!");
        // set tooltip name
        currentTooltipObject.transform.GetChild(1).GetComponent<TMP_Text>().text = blockName;
        // set tooltip text
        currentTooltipObject.transform.GetChild(3).GetComponent<TMP_Text>().text = tooltip;
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
