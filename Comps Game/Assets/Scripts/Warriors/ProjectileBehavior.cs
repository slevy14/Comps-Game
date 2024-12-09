using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour {

    // placed on projectile prefab

    public GameObject target;
    public Vector3 distanceVector;
    public float speed;
    public float rotSpeed;
    public float power;
    public bool isHeal;

    void Awake() {
        // set default values
        speed = 12f;
        rotSpeed = 20f;
    }

    public void InitializeProjectile(GameObject newTarget, float power, bool isHeal) {
        // used like a constructor
        target = newTarget;
        distanceVector = newTarget.transform.position - this.transform.position;
        this.power = power;
        this.isHeal = isHeal;

        // mark that a projectile is active
        LevelController.Instance.activeProjectile = this.gameObject;
    }

    void FixedUpdate() {
        float multSpeed = speed / (0.5f + LevelController.Instance.battleSpeed);
        float multRot = rotSpeed / (1.01f+LevelController.Instance.battleSpeed);
        transform.Rotate(new Vector3(0, 0, rotSpeed));
        if (target != null) {
            // move towards target while it exists
            this.transform.position += multSpeed * Time.fixedDeltaTime * (target.transform.position - this.transform.position).normalized;
            Debug.Log("Projectile moving by " + (multSpeed * Time.fixedDeltaTime * (target.transform.position - this.transform.position).normalized));

            // check if hit target
            if (Vector3.Distance(this.transform.position, target.transform.position) <= 0.3f) {
                // if it's a heal or not blocked, do effect
                if (!target.GetComponent<WarriorBehavior>().GetMagicShield() || isHeal) {
                    target.GetComponent<WarriorBehavior>().DoDamageOrHeal(power, isHeal);
                }
                
                // reset active projectile, destroy this
                LevelController.Instance.activeProjectile = null;
                Destroy(this.gameObject);
            }
        } else { // if target breaks randomly, destroy this
            LevelController.Instance.activeProjectile = null;
            Destroy(this.gameObject);
        }
    }

}
