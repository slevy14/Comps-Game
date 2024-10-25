using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour {

    public GameObject target;
    public Vector3 distanceVector;
    public float speed;
    public float rotSpeed;
    private float power;
    private bool isHeal;

    void Awake() {
        speed = 12f;
        rotSpeed = 20f;
    }

    public void InitializeProjectile(GameObject newTarget, float power, bool isHeal) {
        target = newTarget;
        distanceVector = newTarget.transform.position - this.transform.position;
        this.power = power;
        this.isHeal = isHeal;
    }

    void FixedUpdate() {
        float multSpeed = speed / (1.01f-LevelController.Instance.battleSpeed);
        float multRot = rotSpeed / (1.01f-LevelController.Instance.battleSpeed);
        transform.Rotate(new Vector3(0, 0, rotSpeed));
        if (target != null) {
            this.transform.position += multSpeed * Time.deltaTime * (target.transform.position - this.transform.position).normalized;

            if (Vector3.Distance(this.transform.position, target.transform.position) <= 0.2f) {
                target.GetComponent<WarriorBehavior>().DoDamageOrHeal(power, isHeal);
                Destroy(this.gameObject);
            }
        }
    }

}
