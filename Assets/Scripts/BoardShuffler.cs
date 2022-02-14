using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardShuffler : MonoBehaviour
{
  public List<GamePiece> RemoveNormalPieces(GamePiece[,] allGamePieces)
  {
    int width = allGamePieces.GetLength(0);
    int height = allGamePieces.GetLength(1);
    List<GamePiece> normalPieces = new List<GamePiece>();
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (allGamePieces[i, j] != null)
        {
          Bomb bomb = allGamePieces[i, j].GetComponent<Bomb>();
          Collectible collectible = allGamePieces[i, j].GetComponent<Collectible>();
          if (bomb == null && collectible == null)
          {
            normalPieces.Add(allGamePieces[i,j]);
            allGamePieces[i, j] = null;
          }
        }
      }
    }
    return normalPieces;
  }

  public void ShuffleList(List<GamePiece> gamePieces)
  {
    int max = gamePieces.Count;
    for (int i = 0; i < max-1; i++)
    {
      int r = Random.Range(i, max);
      if (r == i) continue;
      GamePiece temp = gamePieces[i];
      gamePieces[i] = gamePieces[r];
      gamePieces[r] = temp;
    }
  }

  public void MovePieces(GamePiece[,] allPieces, float swapTime = 0.5f)
  {
    int width = allPieces.GetLength(0);
    int height = allPieces.GetLength(1);
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        if (allPieces[i, j] != null)
        {
          allPieces[i,j].Move(i,j,swapTime);
        }
      }
    }
  }
}
