using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Gopher", menuName = "Scriptable Objects/Gopher")]
public class Gopher : ScriptableObject
{
    [Header("MOVEMENT SETTINGS")]
    public float maxSpeed;
    public float acceleration;
    public float deceleration;

    [Header("PLAYER INFO")]
    public string playerName;
    public Sprite gopherSprite;
    public int coins;
    public bool inGame;
    public bool finished;
    public bool didYaWin;
    public Color uiColor;

    public GameObject gopherGame;
}
