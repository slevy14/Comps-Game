using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// SOME HELPER FUNCTIONS USEFUL TO ALL!
// this could (and probably should) just be a static class
// but by making it a monobehaviour object, we can still use update for checks
public class HelperController : MonoBehaviour {

    public static HelperController Instance = null; 

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Awake() {
        CheckSingleton();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            AudioController.Instance.PlaySoundEffect("Click");
        }
    } 


    public bool IsOverUI() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.layer != 5) { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        return raycastResults.Count > 0;
    }

    public int CalculateWarriorStrength(int attackPower, int attackRange, int healPower, int projectilePower, int speed, int maxHealth, int defense) {
        float strength = attackPower*attackRange + healPower*attackRange + projectilePower + speed/10 + maxHealth*(1 + ((float)defense / (float)(defense+maxHealth)));
        return Mathf.RoundToInt(strength);
    }

}
