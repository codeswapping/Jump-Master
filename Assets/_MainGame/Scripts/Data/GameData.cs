using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Data", fileName = "GameData")]
public class GameData : ScriptableObject
{
    public float minSpring, maxSpring;
    public float springForce = 10f;
    public LayerMask playerMask, wallMask;
    public float rayDistance = 10f;

}
