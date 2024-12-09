using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownOptions : MonoBehaviour {

    // used for the sprite selector dropdown in the code editor

    private TMP_Dropdown dropdown;
    private GameObject activeWarrior;

    [SerializeField] DesignerController designerController;

    void Awake() {
        activeWarrior = GameObject.FindGameObjectWithTag("editingObject");
        dropdown = this.gameObject.GetComponent<TMP_Dropdown>();
        SetSpriteOptions(false);
        // dropdown.onValueChanged.AddListener(delegate{UpdateSprite();});
    }

    public void UpdateSprite(int index, bool isEnemy) {
        // update the current warrior sprite to show selected sprite
        // pull from warrior or enemy list as needed
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
        
        // slow down animator
        activeWarrior.transform.GetChild(0).GetComponent<Animator>().speed = 0.4f;
        // value changed! must save warrior manually
        DesignerController.Instance.justSaved = false;
    }

    public void ResetSprite() {
        // this is only ever called for player warriors
        // set default sprite
        dropdown.value = 0;
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[0].sprite;
        activeWarrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = WarriorListController.Instance.spriteDataList[0].animatorController;
    }

    public void SetSpriteOptions(bool isEnemy) {
        // populate sprite selector dropdown with data from warrior or enemy list
        if (!isEnemy) {
            dropdown.ClearOptions();
            foreach (SpriteData sprite in WarriorListController.Instance.spriteDataList) {
                dropdown.options.Add(new TMP_Dropdown.OptionData(sprite.spriteName, sprite.sprite));
            }
        } else {
            dropdown.ClearOptions();
            foreach (SpriteData sprite in EnemyListController.Instance.spriteDataList) {
                dropdown.options.Add(new TMP_Dropdown.OptionData(sprite.spriteName, sprite.sprite));
            }
        }
    }


    // DEPRECATED FUNCTIONS:
    public void UpdateSprite() {
        // update the current warrior sprite to show selected sprite
        int selectedIndex = dropdown.value;
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[selectedIndex].sprite;
        activeWarrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = WarriorListController.Instance.spriteDataList[selectedIndex].animatorController;
        designerController.spriteDataIndex = selectedIndex;
        DesignerController.Instance.justSaved = false;
    }

    public void UpdateSprite(int index) {
        // update the current warrior sprite to show selected sprite
        activeWarrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[index].sprite;
        dropdown.value = index;
        designerController.spriteDataIndex = index;
        DesignerController.Instance.justSaved = false;
    }

}

