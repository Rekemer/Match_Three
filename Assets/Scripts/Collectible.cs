using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece
{
    public bool clearedByBomb = false;
    public bool clearedAtBottom = true;
    void Start()
    {
        matchValue = MatchValue.none;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
