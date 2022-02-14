using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singletone<UIManager>
{
    private CollectionGoalPanel[] m_collectionGoalPanels;
    public GameObject collectionGoalLayout;
    public int collectionGoalBaseWidth = 125;
    
    public ScreenFader ScreenFader;
    public Text nameOfLevel;
    public Text amountsOfMoveText;
    public MessageWindow messageWindow;
    public ScoreMeter ScoreMeter;

    public GameObject movesCounter;

    public Timer timer;
    public override void Awake()
    {
        base.Awake();
        if (messageWindow != null)
        {
            messageWindow.gameObject.SetActive(true);
        }
        if (ScreenFader != null)
        {
            ScreenFader.gameObject.SetActive(true);
        }
    }

    public void SetUpCollectionGoalLayout(CollectionGoal[] collectionGoals, GameObject goalLayout, int spacingWidth )
    {
        if ( goalLayout != null && collectionGoals != null && collectionGoals.Length != 0)
        {
            var rectTransform = goalLayout.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(collectionGoals.Length * collectionGoalBaseWidth,
                rectTransform.sizeDelta.y);
            CollectionGoalPanel[] panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>();
            for (int i = 0; i < panels.Length; i++)
            {
                if (i < collectionGoals.Length && collectionGoals[i] != null)
                {
                    panels[i].gameObject.SetActive(true);
                    panels[i].collectionGoal = collectionGoals[i];
                    panels[i].SetPanel();
                }
                else
                {
                    panels[i].gameObject.SetActive(false);
                }
            }
            
        }
    }

    public void SetUpCollectionGoalLayout(CollectionGoal[] collectionGoals)
    {
        SetUpCollectionGoalLayout(collectionGoals,collectionGoalLayout,collectionGoalBaseWidth);
    }

    public void UpdateCollectionGoalLayout(GameObject goalLayout)
    {
        if (goalLayout != null)
        {
            var panels = goalLayout.GetComponentsInChildren<CollectionGoalPanel>();
            if (panels != null && panels.Length != 0)
            {
                foreach (var mCollectionGoalPanel in panels)
                {
                    if (mCollectionGoalPanel != null && mCollectionGoalPanel.gameObject.activeInHierarchy)
                    {
                        mCollectionGoalPanel.SetPanel();
                    }
                }
            }
        }
    }
    public void UpdateCollectionGoalLayout()
    {
        UpdateCollectionGoalLayout(collectionGoalLayout);
    }

    public void EnableTimer(bool state)
    {
        if (timer != null)
        {
            timer.gameObject.SetActive(state);
        }
    }    
    public void EnableMovesCounter(bool state)
    {
        if (movesCounter != null)
        {
            movesCounter.gameObject.SetActive(state);
        }
    }  
    public void EnableCollectionGoalLayout(bool state)
    {
        if (collectionGoalLayout != null)
        {
           collectionGoalLayout.gameObject.SetActive(state);
        }
    }  
}
