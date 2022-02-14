using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace DefaultNamespace
{
    public class CollectionGoal: MonoBehaviour
    {
        public  GamePiece prefabToCollect;
        [Range(1, 50)]
        public int numberToCollect =5;

        private SpriteRenderer m_spriteRenderer;

        private void Start()
        {
            if (prefabToCollect != null)
            {
                m_spriteRenderer = prefabToCollect.GetComponent<SpriteRenderer>();
            }
        }

        public void Collect(GamePiece gamePiece)
        {
            if (gamePiece == null) return;
            if (gamePiece.GetComponent<SpriteRenderer>().sprite == m_spriteRenderer.sprite &&
                prefabToCollect.matchValue == gamePiece.matchValue)
            {
                numberToCollect--;
                numberToCollect = Mathf.Clamp(numberToCollect, 0, numberToCollect);
            }
        }
    }
}