using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorBehavior : MonoBehaviour {

    // codeable properties
    [Header("Properties")]
    [SerializeField] private string warriorName;
    [SerializeField] private float health;
    [SerializeField] private float defense;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float meleeAttackPower;
    [SerializeField] private float meleeAttackSpeed;
    [SerializeField] private float meleeAttackRange;
    [SerializeField] private float rangedAttackPower;
    [SerializeField] private float rangedAttackSpeed;
    [SerializeField] private float distancedRange;
    [SerializeField] private float specialPower;
    [SerializeField] private float specialSpeed;
    [SerializeField] private float healPower;
    [SerializeField] private float healSpeed;

    // block lists
    private List<BlockData> propertiesData;
    private List<BlockData> moveData;
    private List<BlockData> useWeaponData;
    private List<BlockData> useSpecialData;


    // setup
    void Awake() {
        SetProperties();
    }


    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }


    // FUNCTIONS FROM WHITEBOARD HEADERS
    private void SetProperties() {

    }

    private void Move() {

    }

    private void UseWeapon() {

    }

    private void UseSpecial() {

    }
}
