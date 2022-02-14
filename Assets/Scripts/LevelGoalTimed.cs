using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoalTimed : LevelGoal
{
   
    // private int _maxTime;
    public override void Start()
    {
        LevelCounter = LevelCounter.Timer;
        base.Start();
        if (UIManager.Instance!= null && UIManager.Instance.timer != null)
        {
            UIManager.Instance.timer.Init(timeLeft);
        }

        
    }

    
    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.CurrentScore >= scoreGoals[0];
        }

        return false;
    }

    public override bool IsGameOver()
    {
        int currScore = ScoreManager.Instance.CurrentScore;
        int maxScore = scoreGoals[scoreGoals.Length - 1];
        if (currScore > maxScore) return true;
        return timeLeft <= 0;
    }

   
    
    
}
