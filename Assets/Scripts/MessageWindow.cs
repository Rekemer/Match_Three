using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
   public Image messageImage;
   public Text buttonText;
   public Text messageText;

   public Sprite winSprite;
   public Sprite goalSprite;
   public Sprite loseSprite;

   public Sprite collectionIcon;
   public Sprite timerIcon;
   public Sprite movesIcon;

   public Sprite goalCompleteIcon;
   public Sprite goalFailedIcon;
   
   public Image goalImage;
   public Text goalText;

   public GameObject collectionGoalLayout;
   
   public void ShowMessage(Sprite messageIcon = null, string buttonText = "Start", string messageText = "")
   {
      if (this.messageImage != null)
      {
         this.messageImage.sprite = messageIcon;
      }

      if (this.buttonText != null)
      {
         this.buttonText.text =buttonText;
      }
      if (this.messageText != null)
      {
         this.messageText.text = messageText;
      }
   }

   public void ShowScoreMessage(int scoreGoal)
   {
      string message = "score goal \n" + scoreGoal.ToString();
      ShowMessage(goalSprite, "start",message);
   }

   public void ShowWinMessage()
   {
      ShowMessage(winSprite, "ok","Level\n complete");
   }
   public void ShowLoseMessage()
   {
      ShowMessage(loseSprite, "ok","Level\n failed");
   }

   public void ShowGoal(string caption = "", Sprite icon = null)
   {
      if (caption != "")
      {
         ShowGoalCaption(caption);
      }

      if (icon != null )
      {
         ShowGoalImage(icon);
      }
   }

   public void ShowGoalCaption(string caption = "", int offSetx = 0, int offSety = 0)
   {
      if (goalText != null)
      {
         goalText.text = caption;
         RectTransform rectXform = goalText.GetComponent<RectTransform>();
         rectXform.anchoredPosition += new Vector2(offSetx, offSety);
      }
   }

   public void ShowGoalImage(Sprite icon = null)
   {
      if (goalImage != null)
      {
         goalImage.gameObject.SetActive(true);
         goalImage.sprite = icon;
      }

      if (icon == null)
      {
         goalImage.gameObject.SetActive(false);
      }
   }
   
   public void ShowTimedGoal(int time)
   {
      string caption = time.ToString() + " seconds";
      ShowGoal(caption, timerIcon);
   }

   public void ShowMovesGoal(int moves)
   {
      string caption = moves.ToString() + " moves";
      ShowGoal(caption, movesIcon);
   }

   public void ShowCollectionGoals( bool state = true)
   {
      if (collectionGoalLayout != null)
      {
         collectionGoalLayout.SetActive(state);
      }

      if (state)
      {
         ShowGoal("",collectionIcon);
      }

      
   }
}
 
