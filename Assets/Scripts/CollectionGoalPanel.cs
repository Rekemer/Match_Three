using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class CollectionGoalPanel : MonoBehaviour
{
    public CollectionGoal collectionGoal;
    public Image prefabImage;
    public Text numbersLeftText;

    void Start()
    {
        SetPanel();
    }

    public void SetPanel()
    {
        if (collectionGoal != null && prefabImage != null && numbersLeftText != null)
        {
            SpriteRenderer sprite = collectionGoal.prefabToCollect.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                prefabImage.sprite = sprite.sprite;
                prefabImage.color = sprite.color;
            }

            numbersLeftText.text = collectionGoal.numberToCollect.ToString();
        }
    }

    public void UpdatePanel()
    {
        if (collectionGoal != null && numbersLeftText!= null)
        numbersLeftText.text = collectionGoal.numberToCollect.ToString();
    }
}