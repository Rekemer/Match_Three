using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelCounter
{
    Timer,
    Moves
}
public abstract class  LevelGoal : Singletone<LevelGoal>
{
    public int scoreStars = 0;
    public int[] scoreGoals = new int[3] {1000, 2000, 3000};
    public int movesLeft = 30;
    public int timeLeft = 60;
    public LevelCounter LevelCounter = LevelCounter.Moves;
    private int _maxTime;
    public virtual void Start()
    {
        Init();
        if (LevelCounter == LevelCounter.Timer)
        {
            _maxTime = timeLeft;
        }
    }

    void Init()
    {
        scoreStars = 0;
        for (int i = 0; i < scoreGoals.Length-1; i++)
        {
            if (scoreGoals[i] >= scoreGoals[i + 1])
            {
                Debug.LogWarning("LevelGoal setup is in wrong order");
            }
        }
    }

    int UpdateScore(int score)
    {
        for (int i = 0; i < scoreGoals.Length; i++)
        {
            if (scoreGoals[i] > score)
            {
                return i;
            }
        }

        return scoreGoals.Length;
    }

    public void UpdateScoreStars(int score)
    {
        scoreStars = UpdateScore(score);
    }
    public abstract bool  IsWinner();
    public abstract bool  IsGameOver();
    
    public void StartCountDown()
    {
        StartCoroutine(CountDownRoutine());
    }

    IEnumerator CountDownRoutine()
    {
        while (timeLeft>0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
            if (UIManager.Instance!= null && UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.UpdateTimer(timeLeft);
            }
        }
     
    }
    public void AddTime(int time)
    {
        timeLeft += time;
        timeLeft = Mathf.Clamp(0, _maxTime, timeLeft);
        if (UIManager.Instance!= null && UIManager.Instance.timer != null)
        {
            UIManager.Instance.timer.UpdateTimer(timeLeft);
        }
    }
        

}
