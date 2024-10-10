using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownOptions : MonoBehaviour {

    private TMP_Dropdown dropdown;
    private GameObject activeWarrior;

    [SerializeField] DesignerController designerController;

    void Awake() {
        activeWarrior = GameObject.FindGameObjectWithTag("editingObject");
        dropdown = this.gameObject.GetComponent<TMP_Dropdown>();
        // dropdown.onValueChanged.AddListener(delegate{UpdateSprite();});
    }

    public void UpdateSprite() {
        int selectedIndex = dropdown.value;
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[selectedIndex].sprite;
        designerController.spriteDataIndex = selectedIndex;
        // dropdown.options[selectedIndex].image;
    }

    public void UpdateSprite(int index) {
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[index].sprite;
        dropdown.value = index;
        designerController.spriteDataIndex = index;
    }

    public void UpdateSprite(int index, bool isEnemy) {
        if (!isEnemy) {
            activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[index].sprite;
            dropdown.value = index;
            designerController.spriteDataIndex = index;
        } else { // ENEMY
            activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = EnemyListController.Instance.spriteDataList[index].sprite;
            this.gameObject.SetActive(false);
        }
    }

    public void ResetSprite() {
        dropdown.value = 0;
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[0].sprite;
    }


}

