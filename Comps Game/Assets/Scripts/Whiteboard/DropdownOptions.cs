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
        SetSpriteOptions(false);
        // dropdown.onValueChanged.AddListener(delegate{UpdateSprite();});
    }

    public void UpdateSprite() {
        int selectedIndex = dropdown.value;
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[selectedIndex].sprite;
        activeWarrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = WarriorListController.Instance.spriteDataList[selectedIndex].animatorController;
        designerController.spriteDataIndex = selectedIndex;
        DesignerController.Instance.justSaved = false;
    }

    public void UpdateSprite(int index) {
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[index].sprite;
        dropdown.value = index;
        designerController.spriteDataIndex = index;
        DesignerController.Instance.justSaved = false;
    }

    public void UpdateSprite(int index, bool isEnemy) {
        if (!isEnemy) {
            activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[index].sprite;
            activeWarrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = WarriorListController.Instance.spriteDataList[index].animatorController;
            dropdown.value = index;
            designerController.spriteDataIndex = index;
        } else { // ENEMY
            activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = EnemyListController.Instance.spriteDataList[index].sprite;
            activeWarrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = EnemyListController.Instance.spriteDataList[index].animatorController;
            designerController.spriteDataIndex = index;
        }
        activeWarrior.transform.GetChild(0).GetComponent<Animator>().speed = 0.4f;
        DesignerController.Instance.justSaved = false;
    }

    public void ResetSprite() {
        // this is only ever called for player warriors
        dropdown.value = 0;
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[0].sprite;
        activeWarrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = WarriorListController.Instance.spriteDataList[0].animatorController;
    }

    public void SetSpriteOptions(bool isEnemy) {
        if (!isEnemy) {
            dropdown.ClearOptions();
            foreach (SpriteData sprite in WarriorListController.Instance.spriteDataList) {
                dropdown.options.Add(new TMP_Dropdown.OptionData(sprite.spriteName, sprite.sprite));
            }
            // dropdown.interactable = true;
        } else {
            dropdown.ClearOptions();
            foreach (SpriteData sprite in EnemyListController.Instance.spriteDataList) {
                dropdown.options.Add(new TMP_Dropdown.OptionData(sprite.spriteName, sprite.sprite));
            }
            // dropdown.interactable = false;
        }
    }


}

