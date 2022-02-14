using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RectXformMover : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 posOnScreen;
    public Vector3 endPos;
    public float timeToMove;
    private bool m_isMoving = false;
    private RectTransform _rectTransform;
    
    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Move( Vector3 startPos, Vector3 endPos , float timeToMove)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(startPos, endPos, timeToMove));
        }
    }

    private IEnumerator MoveRoutine(Vector3 startPos, Vector3 destination, float timeToMove)
    {
        if (_rectTransform == null)
        {
            yield break;
        }
        _rectTransform.anchoredPosition = startPos;
        m_isMoving = true;
        bool isReachedDestination = false;
        float elapsedTime = 0;
        while (!isReachedDestination)
        {
            if (Vector3.Distance(destination , _rectTransform.anchoredPosition) < 0.01f)
            {
                isReachedDestination = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            t= t*t*t*(t*(t*6 -15)+10);
            _rectTransform.anchoredPosition = Vector3.Lerp(_rectTransform.anchoredPosition, destination, t);
            yield return null;
        }

        m_isMoving = false;
    }

    public void MoveOn()
    {
        Move(startPos, posOnScreen, timeToMove);
    }
    public void MoveOff()
    {
       Move(posOnScreen, endPos, timeToMove);
    }
}
