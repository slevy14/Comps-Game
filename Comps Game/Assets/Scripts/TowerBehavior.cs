using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TowerBehavior : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform towerRotationPoint; 
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;


    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float bps = 1f; //bullets per second

    private float bpsBase;
    private float targetingRangeBase;

    private Transform target;
    private float timeUntilFire;


    private void Start() {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
    }


    private void Update() {
        if (target == null) {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if(!CheckTargetIsInRange()) {
            target = null;
        } else {
            timeUntilFire += Time.deltaTime;
            if (timeUntilFire >= 1f / bps) {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    private void FindTarget() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(this.transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0) {
            target = hits[0].transform;
        }
    }

    private void Shoot() {
        Debug.Log("shoot");
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, this.transform.rotation);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target);

    }

    private bool CheckTargetIsInRange() {
        return Vector2.Distance(target.position, this.transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget() {
        float angle = Mathf.Atan2(target.position.y - this.transform.position.y, target.position.x - this.transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        towerRotationPoint.rotation = Quaternion.RotateTowards(towerRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    private float CalculateRange() {
        return targetingRangeBase;
    }

    private void OnDrawGizmosSelected() {

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(this.transform.position, transform.forward, targetingRange);

    }


}