using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Object = System.Object;

[RequireComponent(typeof(Image))]
public class Booster : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Image m_image;
    private RectTransform m_rectXform;
    private Vector3 m_startPosition;
    private Board m_board;
    private Tile m_tileTarget;

    public static GameObject ActiveBooster;
    public Text InstructionsText;
    public string instructions = "drag over game piece to remove";

    public bool isEnabled = false;
    public bool isDraggable = true;
    public bool isLocked = false;

    public List<CanvasGroup> CanvasGroups;
    public UnityEvent boostEvent;
    public int boostTime = 15;

    private void Awake()
    {
        m_image = GetComponent<Image>();
        m_rectXform = GetComponent<RectTransform>();
        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
    }

    void Start()
    {
        EnableBooster(false);
    }

    public void ToggleBooster()
    {
        EnableBooster(!isEnabled);
    }

    public void EnableBooster(bool state)
    {
        isEnabled = state;
        if (state)
        {
            DisableOtherBoosters();
            Booster.ActiveBooster = gameObject;
        }
        else if (gameObject == Booster.ActiveBooster)
        {
            Booster.ActiveBooster = null;
        }

        m_image.color = (state) ? Color.white : Color.grey;
        if (InstructionsText != null)
        {
            InstructionsText.gameObject.SetActive(Booster.ActiveBooster != null);
            if (gameObject == Booster.ActiveBooster)
            {
                InstructionsText.text = instructions;
            }
        }
    }

    void DisableOtherBoosters()
    {
        Booster[] allBoosters = GameObject.FindObjectsOfType<Booster>();
        foreach (var booster in allBoosters)
        {
            if (booster != this)
            {
                booster.EnableBooster(false);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked && Camera.main != null)
        {
            Vector3 onScreenPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rectXform, eventData.position, Camera.main,
                out onScreenPosition);
            gameObject.transform.position = onScreenPosition;
            var hit2d = Physics2D.Raycast(onScreenPosition, Vector3.forward, Mathf.Infinity);
            if (hit2d.collider != null)
            {
                m_tileTarget = hit2d.collider.GetComponent<Tile>();
            }
            else
            {
                m_tileTarget = null;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked)
        {
            m_startPosition = gameObject.transform.position;
            EnableCanvasGroups(false);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked)
        {
            gameObject.transform.position = m_startPosition;
            EnableCanvasGroups(true);
            if (m_board != null && m_board.isReffilling)
            {
                return;
            }

            if (m_tileTarget != null)
            {
                boostEvent?.Invoke();
            }

            EnableBooster(false);
            m_tileTarget = null;
            Booster.ActiveBooster = null;
        }
    }

    void EnableCanvasGroups(bool state)
    {
        if (CanvasGroups != null && CanvasGroups.Count > 0)
        {
            foreach (var canvasGroup in CanvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = state;
                }
            }
        }
    }

    public void RemoveOneGamePiece()
    {
        if (m_board != null && m_tileTarget != null)
        {
            m_board.ClearAndRefillBoard(m_tileTarget.xIndex, m_tileTarget.yIndex);
        }
    }

    public void AddTime()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddTime(boostTime);
        }
    }

    public void DropColorBomb()
    {
        if (m_board != null && m_tileTarget != null)
        {
            m_board.MakeColorBombBooster(m_tileTarget.xIndex, m_tileTarget.yIndex);
        }
    }
}