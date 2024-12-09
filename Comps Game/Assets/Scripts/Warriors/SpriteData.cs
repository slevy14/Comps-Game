using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class SpriteData {

    // store all data attached to each sprite
    // name, image, and animator to be loaded for a warrior

    public string spriteName;
    public Sprite sprite;
    public AnimatorOverrideController animatorController;

}
