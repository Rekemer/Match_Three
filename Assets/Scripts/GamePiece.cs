using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum MatchValue
{
    Yellow,
    Blue,
    Magenta,
    Indigo,
    Green,
    Teal,
    Red,
    Cyan,
    Purple,
    Orange,
    none,
    Wild // matches all
        
}
[RequireComponent(typeof(SpriteRenderer))]
public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    private Board m_board;
    private bool m_isMoving = false;
    public InterType interpolation = InterType.SmootherStep;
    public int scoreValue = 20;
    public enum InterType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };

    public MatchValue matchValue;
    public AudioClip clearSound;



    public void Init( Board board)
    {
        m_board = board;
    }
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destX, int destY, float timeToMove)
    {
        if(!m_isMoving) // to invoke one courotine per piece
        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;

        bool reachedDestination = false;
        m_isMoving = true;
        float elapsedTime = 0f;
        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position, destination) < float.Epsilon)
            {
                reachedDestination = true;
                if (m_board != null)
                {
                    m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }
                break;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            switch (interpolation)
            {
                case InterType.Linear:
                    break;
                case InterType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterType.SmoothStep:
                    t = t*t*(3 -2*t);
                    break;
                case InterType.SmootherStep:
                    t = t*t*t*(t*(t*6 -15)+10);
                    break;                  

            }
            transform.position = Vector3.Lerp(startPosition, destination, t);
            yield return null;
        }
        m_isMoving = false;
    }

    public void ChangeColor(GamePiece gamePiece)
    {
        if (gamePiece != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer spriteRendererOfPiece = gamePiece.GetComponent<SpriteRenderer>();
            spriteRenderer.color= spriteRendererOfPiece.color;
            matchValue = gamePiece.matchValue;
        }
    }

    // public void ScorePoints(int multiplier = 1, int bonus = 0)
    // {
    //     if (ScoreManager.Instance != null)
    //     {
    //         ScoreManager.Instance.AddScore(scoreValue * multiplier + bonus);
    //     }
    //
    //     if (SoundManager.Instance != null)
    //     {
    //         SoundManager.Instance.PlayClipAtPoint(clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
    //     }
    // }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 2,(int)transform.position.y, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 2,(int)transform.position.y, 0.5f);
        }
    }
}