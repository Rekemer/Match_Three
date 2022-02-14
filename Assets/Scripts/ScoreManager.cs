using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singletone<ScoreManager>
{
    private int m_score;
    private int m_scoreCounter;
    public int CurrentScore { get => m_score;}
    public Text textScore;
    public int m_increment = 5;

    private void Start()
    {
        UpdateScore(CurrentScore);
    }

    public void UpdateScore(int score)
    {
        if (textScore != null)
        {
            textScore.text = score.ToString();
        }
    }

    public void AddScore(int value)
    {
        m_score += value;
        StartCoroutine(AddScoreRoutine());
    }

    IEnumerator AddScoreRoutine()
    {
        int iterations = 0;
        while (iterations < 100000 && m_score > m_scoreCounter)
        {
            m_scoreCounter += m_increment;
            iterations++;
            UpdateScore(m_scoreCounter);
            yield return null;
        }
        m_scoreCounter =m_score;
        UpdateScore(m_scoreCounter);
    }
}

    
