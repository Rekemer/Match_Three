using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

[RequireComponent(typeof(GamePiece))]
public class BonusTime : MonoBehaviour
{
   [Range(0,5)]
   public int bonus = 5;

   [Range(0, 1f)] public float chanceForBonus = 0.1f;

   public GameObject bonusGlow;
   public GameObject ringGlow;

   public Material[] bonusMaterials;
   private void Start()
   {
      float random = UnityEngine.Random.Range(0f, 1f);
      if (GameManager.Instance != null)
      {
         if (GameManager.Instance.LevelGoal.LevelCounter != LevelCounter.Timer)
         {
            bonus = 0;
         } 
      }
      if (random > chanceForBonus)
      {
         bonus = 0;
      }
      SetActive(bonus != 0);
   }

   public void SetActive(bool state)
   {
      if (ringGlow != null)
      {
         ringGlow.SetActive(state);
      }
      if (bonusGlow != null)
      {
         bonusGlow.SetActive(state);
      }

      if (bonus != 0)
      {
         SetupMaterial(bonus -1, bonusGlow);
      }
   }

   void SetupMaterial(int value, GameObject bonusGlow)
   {

      int clampedValue = Mathf.Clamp(value, 0, bonusMaterials.Length - 1);
      if (bonusMaterials[clampedValue] != null)
      {
         if (bonusGlow != null)
         {
            ParticleSystemRenderer bonusGlowRenderer = bonusGlow.GetComponent<ParticleSystemRenderer>();
            bonusGlowRenderer.material = bonusMaterials[clampedValue];
         }
      }
   }
}
