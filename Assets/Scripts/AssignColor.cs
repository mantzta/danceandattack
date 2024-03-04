using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignColor : MonoBehaviour
{
    public enum ColorType
    {
        Red,
        Blue,
        Bomb,
        PowerUp,
        Attack
    }

    public ColorType color;
}
