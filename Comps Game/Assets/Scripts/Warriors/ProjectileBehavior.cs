using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour {

    public GameObject target;
    public Vector3 distanceVector;
    public float speed;
    public float rotSpeed;
    public float power;
    public bool isHeal;

    void Awake() {
        speed = 12f;
        rotSpeed = 20f;
    }

    public void InitializeProjectile(GameObject newTarget, float power, bool isHeal) {
        target = newTarget;
        distanceVector = newTarget.transform.position - this.transform.position;
        this.power = power;
        this.isHeal = isHeal;
        LevelController.Instance.activeProjectile = this.gameObject;
    }

    void FixedUpdate() {
        float multSpeed = speed / (1.01f+LevelController.Instance.battleSpeed);
        float multRot = rotSpeed / (1.01f+LevelController.Instance.battleSpeed);
        transform.Rotate(new Vector3(0, 0, rotSpeed));
        if (target != null) {
            Debug.Log("mult speed: " + multSpeed);
            this.transform.position += multSpeed * Time.fixedDeltaTime * (target.transform.position - this.transform.position).normalized;
            Debug.Log("Projectile moving by " + (multSpeed * Time.fixedDeltaTime * (target.transform.position - this.transform.position).normalized));

            if (Vector3.Distance(this.transform.position, target.transform.position) <= 0.3f) {
                if (!target.GetComponent<WarriorBehavior>().GetMagicShield() || isHeal) {
                    target.GetComponent<WarriorBehavior>().DoDamageOrHeal(power, isHeal);
                }
                LevelController.Instance.activeProjectile = null;
                Destroy(this.gameObject);
            }
        }
    }

}
