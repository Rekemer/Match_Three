using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}
[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public TileType tileType = TileType.Normal;
    public int breakableValue = 0;
    public Sprite[] breakableSprites;
    public Color normalColor;
    private SpriteRenderer m_spriteRenderer;
    private Board m_board;

    public AudioClip clearSound;
    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
        if (tileType == TileType.Breakable)
        {
            if (breakableSprites[breakableValue] != null)
            {
                m_spriteRenderer.sprite = breakableSprites[breakableValue];
            }
        }
        
    }

    private void OnMouseDown()
    {
        if (m_board != null) m_board.ClickTile(this);
    }

    private void OnMouseEnter()
    {
        if (m_board != null) m_board.DragToTile(this);
    }

    private void OnMouseUp()
    {
        if (m_board != null) m_board.ReleaseTile();
    }

    public void BreakTile()
    {
        if (tileType != TileType.Breakable) return;
        StartCoroutine(BreakTileRoutine());

    }

    IEnumerator BreakTileRoutine()
    {
        breakableValue = Mathf.Clamp(--breakableValue, 0, breakableValue);
        yield return new WaitForSeconds(0.25f);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClipAtPoint(clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
        }
        if (breakableSprites[breakableValue] != null)
        {
            m_spriteRenderer.sprite = breakableSprites[breakableValue];
        }

        if (breakableValue == 0)
        {
            tileType = TileType.Normal;
            m_spriteRenderer.color = normalColor;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
