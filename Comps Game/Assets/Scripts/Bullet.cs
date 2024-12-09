using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    // NOT IN USE!!
    // leftover from old tower defense code!
    // leaving here to not break unity references






    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attribuites")]
    [SerializeField] private float bulletSpeed = 5f;
    // [SerializeField] private int bulletDamage = 1;

    private Transform target;

    private void FixedUpdate() {
        if (!target) {
            return;
        }

        Vector2 direction = (target.position - this.transform.position).normalized;

        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.layer == 6) { // 6 is the enemy layer
            Destroy(this.gameObject);
        }
    }

    public void SetTarget(Transform _target) {
        this.target = _target;
    }

}
