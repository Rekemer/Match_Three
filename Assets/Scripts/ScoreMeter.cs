using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Slider))]
public class ScoreMeter : MonoBehaviour
{
     Slider _slider;
    public ScoreStar[] stars = new ScoreStar[3];
     LevelGoal m_levelGoal;
    private float m_maxScore;

    void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void SetupStars(LevelGoal levelGoal)
    {
        if (levelGoal == null)
        {
            Debug.LogWarning("SCOREMETER invalid level goal");
            return;
        }

        m_levelGoal = levelGoal;
        m_maxScore = m_levelGoal.scoreGoals[m_levelGoal.scoreGoals.Length - 1];
        var width = _slider.GetComponent<RectTransform>().rect.width;
        if (m_maxScore > 0)
        {
            for (int i = 0; i < levelGoal.scoreGoals.Length; i++)
            {
                if (stars[i] != null)
                {
                    float newX = (width * levelGoal.scoreGoals[i] / m_maxScore) - (width * 0.5f);
                    var rectTransform = stars[i].GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);
                    }
                }
            }
        }
    }

    public void UpdateScoreMeter(int currScore, int currStars)
    {
        if (m_levelGoal != null)
        {
            _slider.value = (float) currScore / (float) m_maxScore;
        }
        for (int i = 0; i < currStars; i++)
        {
            if (stars[i] != null)
            {
                stars[i].Activate();
            }
        }
    }
}