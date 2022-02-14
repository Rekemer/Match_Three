using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

// check for no more move condition
public class BoardDeadLock : MonoBehaviour
{
    List<GamePiece> GetColumnOrRow(GamePiece[,] allPieces, int x, int y, bool isRow = true, int listLength = 3)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = 0; i < listLength; i++)
        {
            if (isRow)
            {
                if (x + i < width && y < height && allPieces[x+i,y] !=  null)
                {
                    gamePieces.Add(allPieces[x + i, y]);
                }
            }
            else
            {
                if (x < width && y + i < height&& allPieces[x,y+i] !=  null)
                {
                    gamePieces.Add(allPieces[x, y + i]);
                }
            }
        }


        return gamePieces;
    }

    List<GamePiece> GetMinimumMatches(List<GamePiece> gamePieces, int minMatchLength = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();
        var groups = gamePieces.GroupBy(k => k.matchValue);
        foreach (var group in groups)
        {
            if (group.Count() >= minMatchLength && group.Key != MatchValue.none)
            {
                matches = group.ToList();
            }
        }

        return matches;
    }

    List<GamePiece> GetNeighbors(GamePiece[,] allPieces, int x, int y)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);
        List<GamePiece> neighbors = new List<GamePiece>();
        Vector2[] directions = new Vector2[4]
        {
            new Vector2(-1f, 0f),
            new Vector2(0f, -1f),
            new Vector2(0f, 1f),
            new Vector2(1f, 0f)
        };
        foreach (var dir in directions)
        {
            if (x + (int) dir.x < width && x + (int) dir.x >= 0 && y + (int) dir.y < height && y + (int) dir.y >= 0)
            {
                if (allPieces[x + (int) dir.x, y + (int) dir.y] != null)
                {
                    if (!neighbors.Contains(allPieces[x + (int) dir.x, y + (int) dir.y]))
                    {
                        neighbors.Add(allPieces[x + (int) dir.x, y + (int) dir.y]);
                    }
                }
            }
        }

        return neighbors;
    }

    bool HasMoveAt(GamePiece[,] allPieces, int x, int y, bool isRow = true, int minMatchLength = 3)
    {
        List<GamePiece> pieces = GetColumnOrRow(allPieces, x, y, isRow, minMatchLength);
        List<GamePiece> minimumMatches = GetMinimumMatches(pieces, minMatchLength - 1);
        GamePiece unmathcedPiece = null;
        if (pieces != null && minimumMatches != null)
        {
            if (pieces.Count == minMatchLength && minimumMatches.Count == minMatchLength - 1)
            {
                unmathcedPiece = pieces.Except(minimumMatches).FirstOrDefault();
            }


            if (unmathcedPiece != null)
            {
                List<GamePiece> neighbors =
                    GetNeighbors(allPieces, unmathcedPiece.xIndex, unmathcedPiece.yIndex);
                neighbors = neighbors.Except(minimumMatches).ToList();
                neighbors = neighbors.FindAll(t => t.matchValue == minimumMatches[0].matchValue);
                minimumMatches = minimumMatches.Union(neighbors).ToList();
            }

            if (minimumMatches.Count >= minMatchLength)
            {
                //string rowColStr = isRow ? "row" : "column";
                // Debug.Log("================AVAILABLE MOVE================");
                // Debug.Log(
                //     $"you can move {minimumMatches[0].matchValue} piece to {unmathcedPiece.xIndex} , {unmathcedPiece.yIndex} to form matching {rowColStr}");
                return true;
            }
        }

        return false;
    }

    public bool IsDeadLock(GamePiece[,] allPieces, int matchLength = 3)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);
        bool isDeadLock = true;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (HasMoveAt(allPieces, i, j, true, matchLength) || HasMoveAt(allPieces, i, j, false, matchLength))
                {
                    isDeadLock = false;
                }
            }
        }

        if (isDeadLock)
        {
            Debug.Log("============== BOARD DEADLOCKED ==============");
        }

        return isDeadLock;
    }
}