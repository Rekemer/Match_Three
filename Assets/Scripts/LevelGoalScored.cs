using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalScored : LevelGoal
{
    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.CurrentScore >= scoreGoals[0];
        }

        return false;
    }

    public override void Start()
    {
        LevelCounter = LevelCounter.Moves;
        base.Start();
    }

    public override bool IsGameOver()
    {
        int currScore = ScoreManager.Instance.CurrentScore;
        int maxScore = scoreGoals[scoreGoals.Length - 1];
        if (currScore > maxScore) return true;
        return movesLeft == 0;
    }
}
