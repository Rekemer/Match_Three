using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class LevelGoalCollected : LevelGoal
{
    public CollectionGoal[] levelGoals;
    
    public void UpdateGoals(GamePiece gamePieceToCheck)
    {
        if (gamePieceToCheck != null)
        {
            foreach (var goal in levelGoals)
            {
                if (goal != null)
                {
                    goal.Collect(gamePieceToCheck);
                }
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCollectionGoalLayout();
        }
    }
    public bool AreGoalsComplete(CollectionGoal[] collectionGoals)
    {
        if (collectionGoals == null) return false;
        if (collectionGoals.Length == 0) return false;
        foreach (var goal in collectionGoals)
        {
            if (goal == null) return false;
            if (goal.numberToCollect != 0)
            {
                return false;
            }
        }

        return true;
    }
    public override bool IsWinner()
    {
        if (ScoreManager.Instance != null)
        {
            return ScoreManager.Instance.CurrentScore >= scoreGoals[0] && AreGoalsComplete(levelGoals);
        }

        return false;
    }

    public override bool IsGameOver()
    {
        if (ScoreManager.Instance != null && AreGoalsComplete(levelGoals))
        {
            int maxScore = scoreGoals[scoreGoals.Length - 1];
            if (ScoreManager.Instance.CurrentScore >= maxScore)
            {
                return true;
            }
        }

        if (LevelCounter == LevelCounter.Timer)
        {
            return timeLeft <= 0;
        }
        else
        {
            return movesLeft <=0;
        }
       
    }
}
