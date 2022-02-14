using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BombType
{
    none, 
    column,
    row,
    adjacent,
    color
}
public class Bomb : GamePiece
{
    public BombType BombType;
}
