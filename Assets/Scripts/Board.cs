using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using DefaultNamespace;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;


[RequireComponent(typeof(BoardDeadLock))]
[RequireComponent(typeof(BoardShuffler))]
public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int borderSize;
    public float swapTime = 0.5f;
    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] gamePiecePrefabs;

    public GameObject[] adjacentBombPrefabs;
    public GameObject[] rowBombPrefabs;
    public GameObject[] columnBombPrefabs;
    public GameObject colorBombPrefab;
    public int fillYOffset = 10;
    public float moveTime = 0.5f;


    private GameObject m_clickedTileBomb;
    private GameObject m_targetTileBomb;

    public StartingObject[] startingTiles;
    public StartingObject[] startingPieces;
    public int maxCollectibles = 3;
    public int collectiblesCount = 0;
    [Range(0, 1)] public float chanceForCollectible = 0.1f;
    public GameObject[] collectiblePrefabs;

    private int m_scoreMultiplier;

    private bool m_playerInputEnabled = true;
    public bool isReffilling = false;
    private Tile[,] m_allTiles;
    private GamePiece[,] m_allGamePieces;
    private ParticleManager m_particleManager;
    private Tile m_clickedTile;
    private Tile m_targetTile;
    private BoardDeadLock m_deadLock;
    private BoardShuffler m_boardShuffler;

    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;

        public int posX;
        public int posY;
        public int posZ;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();
        m_deadLock = GetComponent<BoardDeadLock>();
        m_boardShuffler = GetComponent<BoardShuffler>();
        //SetUpBoard();
        // ClearPieceAt(3,5);
        // ClearPieceAt(1,4);
        // HighlightMatches();
    }

    public void SetUpBoard()
    {
        SetUpTiles();
        SetUpGamePieces();
        List<GamePiece> startingCollectibles = FindCollectibles();
        collectiblesCount = startingCollectibles.Count;

        SetupCamera();
        FillBoard(fillYOffset, moveTime);
    }

    void SetUpTiles()
    {
        foreach (var startingTile in startingTiles)
        {
            MakeTile(startingTile.prefab, startingTile.posX, startingTile.posY, startingTile.posZ);
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i, j] == null)
                    MakeTile(tileNormalPrefab, i, j);
            }
        }
    }

    private void MakeTile(GameObject prefab, int i, int j, int z = 0)
    {
        if (prefab != null && IsWithinBounds(i, j))
        {
            GameObject tile = Instantiate(prefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
            tile.name = "Tile (" + i + "," + j + ")";
            m_allTiles[i, j] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            m_allTiles[i, j].Init(i, j, this);
        }
    }

    void SetUpGamePieces()
    {
        foreach (StartingObject piece in startingPieces)
        {
            if (piece != null)
            {
                GameObject sPiece = Instantiate(piece.prefab, new Vector3(piece.posX, piece.posY, 0),
                    Quaternion.identity) as GameObject;
                MakeGamePiece(sPiece, piece.posX, piece.posY, moveTime, fillYOffset);
            }
        }
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float) (width - 1) / 2f, (float) (height - 1) / 2f, -10f);
        float aspectRatio = (float) Screen.width / (float) Screen.height;
        float verticalSize = (float) height / 2f + (float) borderSize;
        float horizontalSize = ((float) width / 2f + (float) borderSize) / aspectRatio;
        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    GameObject GetRandomObject(GameObject[] prefabs)
    {
        int ind = Random.Range(0, prefabs.Length);
        if (prefabs[ind] == null)
        {
            Debug.LogWarning("board.GetRandomObject at index" + ind + "does not contain a valid GameObject!");
        }

        return prefabs[ind];
    }

    GameObject GetRandomGamePiece()
    {
        return GetRandomObject(gamePiecePrefabs);
    }

    GameObject GetRandomCollectible()
    {
        return GetRandomObject(collectiblePrefabs);
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD:  Invalid GamePiece");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }

        gamePiece.SetCoord(x, y);
    }

    void FillBoard(int falseYOffSet = 0, float moveTime = 0)
    {
        int maxIterations = 100;
        int iteration = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    //GamePiece piece = null;
                    if (j == height - 1 && CanAddCollectible())
                    {
                        FillRandomCollectibleAt(i, j, falseYOffSet, moveTime);
                        collectiblesCount++;
                    }
                    else
                    {
                        FillRandomGamePieceAt(i, j, falseYOffSet, moveTime);
                        iteration = 0;
                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAt(i, j);

                            FillRandomGamePieceAt(i, j, falseYOffSet, moveTime);
                            iteration++;
                            if (iteration >= maxIterations)
                            {
                                Debug.Log("break===========================");
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void BreakTileAt(int i, int j)
    {
        var breakableTile = m_allTiles[i, j];
        if (breakableTile != null)
        {
            if (m_particleManager != null && breakableTile.tileType == TileType.Breakable)
            {
                m_particleManager.BreakTileFXAt(breakableTile.breakableValue, breakableTile.xIndex,
                    breakableTile.yIndex, 0);
            }

            m_allTiles[i, j].BreakTile();
        }
    }

    private void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                BreakTileAt(gamePiece.xIndex, gamePiece.yIndex);
            }
        }
    }

    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downardMatches = FindMatches(x, y, new Vector2(-0, -1), minLength);

        if (leftMatches == null) leftMatches = new List<GamePiece>();
        if (downardMatches == null) downardMatches = new List<GamePiece>();
        return (leftMatches.Count > 0 || downardMatches.Count > 0);
    }

    private GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
            MakeGamePiece(randomPiece, x, y, moveTime, falseYOffset);
            return randomPiece.GetComponent<GamePiece>();
        }


        return null;
    }

    private GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (IsWithinBounds(x, y))
        {
            GameObject collectible = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity);
            MakeGamePiece(collectible, x, y, moveTime, falseYOffset);

            return collectible.GetComponent<GamePiece>();
        }

        return null;
    }

    private void MakeGamePiece(GameObject prefab, int x, int y, float moveTime = 0.1f, int falseYOffset = 0)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            prefab.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);
            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
            }

            prefab.transform.parent = transform;
        }
    }

    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
            // Debug.Log("clicked tile: " + tile.name);
        }
    }

    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }

    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnabled && !GameManager.Instance.GameOver)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];
            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> colorBombMatches = new List<GamePiece>();
                if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
                {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorBombMatches = FindAllMatchValue(clickedPiece.matchValue);
                }
                else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorBombMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (!colorBombMatches.Contains(m_allGamePieces[i, j]))
                            {
                                colorBombMatches.Add(m_allGamePieces[i, j]);
                            }
                        }
                    }
                }

                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && colorBombMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                }
                else
                {
                    

                    yield return new WaitForSeconds(swapTime);
                    Vector2 swapDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex,
                        targetTile.yIndex - clickedTile.yIndex);
                    m_clickedTileBomb = DropBomb(clickedPieceMatches, swapDirection, clickedTile.xIndex,
                        clickedTile.yIndex);
                    m_targetTileBomb = DropBomb(targetPieceMatches, swapDirection, targetTile.xIndex,
                        targetTile.yIndex);
                    if (m_clickedTileBomb != null && targetPiece != null)
                    {
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(clickedBombPiece))
                        {
                            m_clickedTileBomb.GetComponent<GamePiece>().ChangeColor(targetPiece);
                        }
                    }

                    if (m_targetTileBomb != null && clickedPiece != null)
                    {
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(targetBombPiece))
                        {
                            targetBombPiece.GetComponent<GamePiece>().ChangeColor(clickedPiece);
                        }
                    }

                   
                    yield return StartCoroutine(ClearAndRefillBoardRoutine(clickedPieceMatches.Union(targetPieceMatches)
                        .ToList().Union(colorBombMatches)
                        .ToList()));
                    
                    if (GameManager.Instance != null)
                    {
                       
                        GameManager.Instance.UpdateMoves();
                    }
                }
            }
        }
    }

    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else return null;

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * i;
            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if (nextPiece == null) break;
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) &&
                    nextPiece.matchValue != MatchValue.none)
                {
                    matches.Add(nextPiece);
                }
                else break;
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }

        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece>
            upwardMatches =
                FindMatches(startX, startY, new Vector2(0, 1), 2); // 2 since one can be higher and one be below
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);
        // to avoid errors in loops(union)
        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combineMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combineMatches.Count >= minLength) ? combineMatches : null;
    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece>
            rightMatches =
                FindMatches(startX, startY, new Vector2(1, 0), 2); // 2 since one can be higher and one be below
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        // to avoid errors in loops(union)
        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combineMatches = rightMatches.Union(leftMatches).ToList();
        return (combineMatches.Count >= minLength) ? combineMatches : null;
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    private void HighlightMatchesAt(int x, int y)
    {
        // just make sure it is invinsible
        HighlightOff(x, y);

        var combineMatches = FindMatchesAt(x, y);

        if (combineMatches.Count > 0)
        {
            foreach (GamePiece piece in combineMatches)
            {
                HighlightOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    private void HighlightOn(int x, int y, Color col)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = col;
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                HighlightOn(gamePiece.xIndex, gamePiece.yIndex, gamePiece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);
        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }

        var combineMatches = horizMatches.Union(vertMatches).ToList();
        return combineMatches;
    }

    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        foreach (GamePiece gamePiece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(gamePiece.xIndex, gamePiece.yIndex, minLength)).ToList();
        }

        return matches;
    }

    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if (pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        //HighlightOff(x,y);
    }

    void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> gamePiecesToBomb)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                if (m_particleManager != null)
                {
                    if (gamePiecesToBomb.Contains(gamePiece))
                    {
                        m_particleManager.BombFXAt(gamePiece.xIndex, gamePiece.yIndex);
                    }
                    else
                    {
                        m_particleManager.ClearPieceFXAt(gamePiece.xIndex, gamePiece.yIndex, 0);
                    }
                }

                ClearPieceAt(gamePiece.xIndex, gamePiece.yIndex);
                // destroy is called in the end of the frame, that is why you can address gameppiece
                int bonus = 0;
                if (gamePieces.Count >= 4)
                {
                    bonus = 20;
                }

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ScorePoints(gamePiece, m_scoreMultiplier, bonus);
                    GameManager.Instance.UpdateLevelGoals(gamePiece);
                    BonusTime time = gamePiece.GetComponent<BonusTime>();
                    if (time != null)
                    {
                        GameManager.Instance.AddTime(time.bonus);
                    }
                    
                }   
                //gamePiece.ScorePoints(m_scoreMultiplier, bonus);
            }
        }
    }


    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
                if (m_particleManager != null)
                {
                    m_particleManager.ClearPieceFXAt(i, j);
                }
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null && m_allTiles[column, i].tileType != TileType.Obstacle)
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);
                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }

                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }

        return movingPieces;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }

    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (!columns.Contains(gamePiece.xIndex))
            {
                columns.Add(gamePiece.xIndex);
            }
        }

        return columns;
    }

    List<GamePiece> CollapseColumn(List<int> columnsToCollapse)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        foreach (var column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        List<int> columnsToCollapse = GetColumns(gamePieces);
        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    public  void ClearAndRefillBoard(int x, int y)
    {
        if (IsWithinBounds(x, y))
        {
            GamePiece pieceToClear = m_allGamePieces[x, y];
            List<GamePiece> gamePieces = new List<GamePiece>();
            gamePieces.Add(pieceToClear);
            ClearAndRefillBoard(gamePieces);
        }
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        // clear and collapse
        m_playerInputEnabled = false;
        isReffilling = true;
        List<GamePiece> matches = gamePieces;
        m_scoreMultiplier = 0;
        do
        {
            m_scoreMultiplier++;
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;
            // refill
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();
            yield return new WaitForSeconds(0.2f);
        } while (matches.Count != 0);

        if (m_deadLock.IsDeadLock(m_allGamePieces, 3))
        {
            yield return new WaitForSeconds(1f);
            //ClearBoard();
            yield return StartCoroutine(ShuffleBoardRoutine());
            yield return new WaitForSeconds(1f);
            //StartCoroutine(RefillRoutine());
        }

        m_playerInputEnabled = true;
        isReffilling = false;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, moveTime);
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        yield return new WaitForSeconds(0.2f);
        bool isFinished = false;
        while (!isFinished)
        {
            // get  list of bombed pieces
            List<GamePiece> gamePiecesToBomb = new List<GamePiece>();
            gamePiecesToBomb = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(gamePiecesToBomb).ToList();
            // to have series of explosions
            gamePiecesToBomb = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(gamePiecesToBomb).ToList();
            // to remove collectibles at bottom let them fall 

            var collectedPieces = FindCollectiblesAt(0, true);
            var allCollectedPieces = FindCollectibles();
            var blockers = gamePieces.Intersect(allCollectedPieces).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();
            collectiblesCount -= collectedPieces.Count;
            gamePieces = gamePieces.Union(collectedPieces).ToList();
            List<int> columnsToCollapse = GetColumns(gamePieces);
            ClearPieceAt(gamePieces, gamePiecesToBomb);


            BreakTileAt(gamePieces);
            if (m_clickedTileBomb != null)
            {
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }

            if (m_targetTileBomb != null)
            {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(columnsToCollapse);
            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }


            yield return new WaitForSeconds(0.2f);
            // destroy collectibles
            matches = FindMatchesAt(movingPieces);
            collectedPieces = FindCollectiblesAt(0, true);
            matches = matches.Union(collectedPieces).ToList();


            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                m_scoreMultiplier++;
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBonusSound();
                }

                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }

        yield return null;
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (var gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                if (gamePiece.transform.position.y - (float) gamePiece.yIndex > float.Epsilon)
                {
                    return false;
                }

                if (gamePiece.transform.position.x - (float) gamePiece.xIndex > float.Epsilon)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] != null)
            {
                gamePieces.Add(m_allGamePieces[i, row]);
            }
        }

        return gamePieces;
    }

    public List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[column, i] != null)
            {
                gamePieces.Add(m_allGamePieces[column, i]);
            }
        }

        return gamePieces;
    }

    public List<GamePiece> GetAdjacentGamePieces(int x, int y, int offSet = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = x - offSet; i <= x + offSet; i++)
        {
            for (int j = y - offSet; j <= y + offSet; j++)
            {
                if (IsWithinBounds(i, j))
                {
                    gamePieces.Add(m_allGamePieces[i, j]);
                }
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allGamePieces = new List<GamePiece>();
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> gamePiecesToBomb = new List<GamePiece>();
                Bomb bomb = piece.GetComponent<Bomb>();
                if (bomb != null)
                {
                    switch (bomb.BombType)
                    {
                        case BombType.adjacent:
                            gamePiecesToBomb = GetAdjacentGamePieces(piece.xIndex, piece.yIndex, 1);
                            break;
                        case BombType.column:
                            gamePiecesToBomb = GetColumnPieces(piece.xIndex);
                            break;
                        case BombType.row:
                            gamePiecesToBomb = GetRowPieces(piece.yIndex);
                            break;
                        case BombType.color:
                            break;
                    }

                    allGamePieces = allGamePieces.Union(gamePiecesToBomb).ToList();
                    allGamePieces = RemoveCollectibles(allGamePieces);
                }
            }
        }

        return allGamePieces;
    }

    private List<GamePiece> RemoveCollectibles(List<GamePiece> allGamePieces)
    {
        List<GamePiece> allCollectibles = FindCollectibles();
        List<GamePiece> collectibles = new List<GamePiece>();
        foreach (GamePiece piece in allCollectibles)
        {
            Collectible collectible = piece.GetComponent<Collectible>();
            if (collectible != null)
            {
                if (collectible.clearedByBomb == false)
                {
                    collectibles.Add(piece);
                }
            }
        }

        return allGamePieces.Except(collectibles).ToList();
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool horizontal = false;
        bool vertical = false;
        int startX = -1;
        int startY = -1;
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (startX == -1 || startY == -1)
                {
                    startX = piece.xIndex;
                    startY = piece.yIndex;
                    continue;
                }

                if (startX != piece.xIndex && piece.yIndex == startY)
                {
                    horizontal = true;
                }

                if (startY != piece.yIndex && piece.xIndex == startX)
                {
                    vertical = true;
                }
            }
        }

        return (horizontal && vertical);
    }

    GameObject MakeBomb(GameObject prefab, int x, int y)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
        }

        return null;
    }

    public void MakeColorBombBooster(int x, int y)
    {
        if (IsWithinBounds(x, y))
        {
            GamePiece gamePieceToReplace = m_allGamePieces[x, y];
            if (gamePieceToReplace != null)
            {
                ClearPieceAt(x,y);
                GameObject bombObject = MakeBomb(colorBombPrefab, x, y);
                ActivateBomb(bombObject);
            }
        }
    }
    GameObject DropBomb(List<GamePiece> gamePieces, Vector2 swapDirection, int x, int y)
    {
        GameObject bombToDrop = null;
        MatchValue matchValue = FindMatchValue(gamePieces);
        if (gamePieces.Count >= 5 && matchValue != MatchValue.none)
        {
            if (IsCornerMatch(gamePieces))
            {
                GameObject adjacentBombPrefab = FindGameObjectByMatchValue(adjacentBombPrefabs, matchValue);
                //drop adjacent bomb
                if (adjacentBombPrefab != null)
                {
                    bombToDrop = MakeBomb(adjacentBombPrefab, x, y);
                }
            }
            else
            {
                if (colorBombPrefab != null)
                {
                    bombToDrop = MakeBomb(colorBombPrefab, x, y);
                }
            }
        }
        else if (gamePieces.Count == 4 && matchValue != MatchValue.none)
        {
            if (swapDirection.x != 0)
            {
                GameObject rowBombPrefab = FindGameObjectByMatchValue(rowBombPrefabs, matchValue);
                if (rowBombPrefab != null)
                {
                    bombToDrop = MakeBomb(rowBombPrefab, x, y);
                }
            }
            else if (swapDirection.y != 0)
            {
                GameObject columnBombPrefab = FindGameObjectByMatchValue(rowBombPrefabs, matchValue);
                if (columnBombPrefab != null)
                {
                    bombToDrop = MakeBomb(columnBombPrefab, x, y);
                }
            }
        }


        return bombToDrop;
    }

    void ActivateBomb(GameObject bomb)
    {
        int x = (int) bomb.transform.position.x;
        int y = (int) bomb.transform.position.y;
        if (IsWithinBounds(x, y))
        {
            m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue(MatchValue value)
    {
        List<GamePiece> matches = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] != null)
                {
                    if (m_allGamePieces[i, j].matchValue == value)
                    {
                        matches.Add(m_allGamePieces[i, j]);
                    }
                }
            }
        }

        return matches;
    }

    bool IsColorBomb(GamePiece piece)
    {
        Bomb bomb = piece.GetComponent<Bomb>();
        if (bomb != null)
        {
            return bomb.BombType == BombType.color;
        }

        return false;
    }

    List<GamePiece> FindCollectiblesAt(int row, bool destroyedAtBottomOnly = false)
    {
        List<GamePiece> collectiblesInRow = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] != null)
            {
                Collectible collectible = m_allGamePieces[i, row].GetComponent<Collectible>();
                if (collectible != null)
                {
                    if (!destroyedAtBottomOnly || (destroyedAtBottomOnly && collectible.clearedByBomb))
                        collectiblesInRow.Add(m_allGamePieces[i, row]);
                }
            }
        }

        return collectiblesInRow;
    }

    List<GamePiece> FindCollectibles()
    {
        List<GamePiece> allCollectibles = new List<GamePiece>();
        for (int i = 0; i < height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i);
            allCollectibles = allCollectibles.Union(collectibleRow).ToList();
        }

        return allCollectibles;
    }

    bool CanAddCollectible()
    {
        return Random.Range(0, 1f) <= chanceForCollectible && collectiblesCount < maxCollectibles &&
               collectiblePrefabs.Length > 0;
    }

    MatchValue FindMatchValue(List<GamePiece> gamePieces)
    {
        foreach (var gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                return gamePiece.matchValue;
            }
        }

        return MatchValue.none;
    }

    GameObject FindGameObjectByMatchValue(GameObject[] gameObjects, MatchValue matchValue)
    {
        if (matchValue == null) return null;
        foreach (var go in gameObjects)
        {
            GamePiece piece = go.GetComponent<GamePiece>();
            if (piece.matchValue == matchValue)
            {
                return go;
            }
        }

        return null;
    }

    public void IsDeadLockTest()
    {
        m_deadLock.IsDeadLock(m_allGamePieces, 3);
    }

    void FillBoardFromList(List<GamePiece> gamePieces)
    {
        Queue<GamePiece> queue = new Queue<GamePiece>(gamePieces);
        int iterations = 0;
        int maxIterations = 100;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    m_allGamePieces[i, j] = queue.Dequeue();
                    while (HasMatchOnFill(i, j))
                    {
                        queue.Enqueue(m_allGamePieces[i, j]);
                        m_allGamePieces[i, j] = queue.Dequeue();
                        iterations++;
                        if (iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    void ShuffleBoard()
    {
        if (m_playerInputEnabled)
        {
            StartCoroutine(ShuffleBoardRoutine());
        }
    }

    IEnumerator ShuffleBoardRoutine()
    {
        List<GamePiece> allPieces = new List<GamePiece>();
        foreach (var gamePiece in m_allGamePieces)
        {
            allPieces.Add(gamePiece);
        }

        while (!IsCollapsed(allPieces))
        {
            yield return null;
        }

        List<GamePiece> gamePieces = m_boardShuffler.RemoveNormalPieces(m_allGamePieces);
        m_boardShuffler.ShuffleList(gamePieces);
        FillBoardFromList(gamePieces);
        m_boardShuffler.MovePieces(m_allGamePieces);
        List<GamePiece> matches = FindAllMatches();
        StartCoroutine(ClearAndRefillBoardRoutine(matches));
    }
}