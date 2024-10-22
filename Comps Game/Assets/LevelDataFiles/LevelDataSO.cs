using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/LevelData")]
public class LevelDataSO : ScriptableObject {

    [Header("Title")]
    public string levelName;
    public int levelNumber;

    [Header("Enemies")]
    public List<int> availableEnemyIndices;
    public List<WarriorOnGrid> enemyPlacementsList;

    [Header("Progression")]
    public List<int> availablePropertiesIndices;
    public List<int> availableBehaviorsIndices;
    public bool isMoveHeaderAvailable = false;
    public bool isUseSpecialHeaderAvailable = false;

    [Header("Constraints")]
    public int maxTotalStrength;
    public int maxBlocks;
    public int actionPoints;
    public int maxWarriorsToPlace;

    [Header("Tutorial")]
    public List<string> tutorialDialog;
}
