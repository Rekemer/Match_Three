using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(LevelGoal))]
public class GameManager : Singletone<GameManager>
{
    // public int turnsLeft = 30;
    // public int scoreToBeat = 10000;

    


    private Board m_board;
    private bool m_IsWinner = false;
    private bool m_IsReadyToBegin = false;
    private bool m_IsGameOver = false;
    private bool m_readyToReload = false;

    private LevelGoal m_levelGoal;

    // private LevelGoalTimed m_LevelGoalTimed;
    private LevelGoalCollected m_levelGoalCollected;

    public LevelGoal LevelGoal
    {
        get => m_levelGoal;
    }

    public bool GameOver
    {
        get => m_IsGameOver;
    }

    public override void Awake()
    {
        base.Awake();
        m_levelGoal = GetComponent<LevelGoal>();
        //m_LevelGoalTimed = GetComponent<LevelGoalTimed>();
        m_levelGoalCollected = GetComponent<LevelGoalCollected>();
        m_board = GameObject.FindObjectOfType<Board>().GetComponent<Board>();
    }

    void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ScoreMeter.SetupStars(m_levelGoal);
            if (UIManager.Instance.nameOfLevel != null)
            {
                UIManager.Instance.nameOfLevel.text = SceneManager.GetActiveScene().name;
            }

            if (m_levelGoalCollected != null)
            {
                UIManager.Instance.EnableCollectionGoalLayout(true);
                UIManager.Instance.SetUpCollectionGoalLayout(m_levelGoalCollected.levelGoals);
            }
            else
            {
                UIManager.Instance.EnableCollectionGoalLayout(false);
            }

            bool useTimer = (m_levelGoal.LevelCounter == LevelCounter.Timer);
            UIManager.Instance.EnableTimer(useTimer);
            UIManager.Instance.EnableMovesCounter(!useTimer);
        }


        m_levelGoal.movesLeft++;
        UpdateMoves();


        StartCoroutine(ExecuteGameLoop());
    }

    public void UpdateMoves()
    {
        if (m_levelGoal.LevelCounter == LevelCounter.Moves)
        {
            m_levelGoal.movesLeft--;
            if (UIManager.Instance != null && UIManager.Instance.amountsOfMoveText != null)
            {
                UIManager.Instance.amountsOfMoveText.text = m_levelGoal.movesLeft.ToString();
            }
        }
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("WaitForBoardRoutine", 0.5);
        yield return StartCoroutine("EndGameRoutine");
    }

    public void BeginGame()
    {
        m_IsReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                int maxGoal = m_levelGoal.scoreGoals.Length - 1;
                UIManager.Instance.messageWindow.ShowScoreMessage(m_levelGoal.scoreGoals[maxGoal]);

                if (m_levelGoal.LevelCounter == LevelCounter.Timer)
                {
                    UIManager.Instance.messageWindow.ShowTimedGoal(m_levelGoal.timeLeft);
                }
                else if (m_levelGoal.LevelCounter == LevelCounter.Moves)
                {
                    UIManager.Instance.messageWindow.ShowMovesGoal(m_levelGoal.movesLeft);
                }

                if (m_levelGoalCollected != null)
                {
                    UIManager.Instance.messageWindow.ShowCollectionGoals(true);
                    GameObject layout = UIManager.Instance.messageWindow.collectionGoalLayout;
                    if (layout != null)
                    {
                        UIManager.Instance.SetUpCollectionGoalLayout(m_levelGoalCollected.levelGoals, layout, 80);
                    }
                }
            }
        }


        while (!m_IsReadyToBegin)
        {
            yield return null;
        }

        if (UIManager.Instance != null && UIManager.Instance.ScreenFader != null)
        {
            UIManager.Instance.ScreenFader.FadeOff();
        }

        yield return new WaitForSeconds(0.5f);
        if (m_board != null)
        {
            m_board.SetUpBoard();
        }
    }

    IEnumerator PlayGameRoutine()
    {
        if (m_levelGoal.LevelCounter == LevelCounter.Timer)
        {
            m_levelGoal.StartCountDown();
        }

        while (!m_IsGameOver)
        {
            m_IsWinner = m_levelGoal.IsWinner();
            m_IsGameOver = m_levelGoal.IsGameOver();
            yield return null;
        }
    }

    IEnumerator EndGameRoutine()
    {
        // to reload few times, not just once and at first button-pressing it will be true
        m_readyToReload = false;
        if (UIManager.Instance != null && UIManager.Instance.ScreenFader != null)
        {
            UIManager.Instance.ScreenFader.FadeOn();
        }

        if (m_IsWinner)
        {
            ShowWinScreen();
        }
        else
        {
            ShowLoseScreen();
        }

        while (!m_readyToReload)
        {
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowLoseScreen()
    {
        if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
        {
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowLoseMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoals(false);
            string caption = "";
            if (m_levelGoal.LevelCounter == LevelCounter.Timer)
            {
                caption = "Out of time!";
            }
            else
            {
                caption = "Out of moves!";
            }
            UIManager.Instance.messageWindow.ShowGoalCaption(caption,0,70);
            if (UIManager.Instance.messageWindow.loseSprite != null)
            {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalFailedIcon);
            }
            SoundManager.Instance.PlayLoseSound();
        }
    }

    private void ShowWinScreen()
    {
        if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
        {
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowWinMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoals(false);
            if (ScoreManager.Instance != null)
            {
                string scoreStr = "You Scored \n" + ScoreManager.Instance.CurrentScore.ToString() +  " points!";
                UIManager.Instance.messageWindow.ShowGoalCaption(scoreStr,0,70);
            }
           
            if (UIManager.Instance.messageWindow.goalCompleteIcon != null)
            {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalCompleteIcon);
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWinSound();
            }
            
        }
    }

    public void ReloadScene()
    {
        m_readyToReload = true;
    }

    IEnumerator WaitForBoardRoutine(float delay = 0)
    {
        if (UIManager.Instance != null && UIManager.Instance.timer != null &&
            m_levelGoal.LevelCounter == LevelCounter.Timer)
        {
            UIManager.Instance.timer.FadeOff();
            UIManager.Instance.timer.isPaused = true;
        }

        yield return new WaitForSeconds(m_board.swapTime);
        if (m_board != null)
        {
            while (m_board.isReffilling == true)
            {
                yield return null;
            }
        }


        yield return new WaitForSeconds(delay);
    }

    public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0)
    {
        if (piece == null) return;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);
            m_levelGoal.UpdateScoreStars(ScoreManager.Instance.CurrentScore);
            if (UIManager.Instance != null && UIManager.Instance.ScoreMeter != null)
            {
                UIManager.Instance.ScoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore,
                    m_levelGoal.scoreStars);
            }
        }

        if (SoundManager.Instance != null && piece.clearSound != null)
        {
            SoundManager.Instance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
        }
    }

    public void AddTime(int time)
    {
        if (m_levelGoal.LevelCounter == LevelCounter.Timer)
        {
            m_levelGoal.AddTime(time);
        }
    }

    public void UpdateLevelGoals(GamePiece gamePiece)
    {
        if (gamePiece != null)
        {
            if (m_levelGoalCollected != null)
            {
                m_levelGoalCollected.UpdateGoals(gamePiece);
            }
        }
    }
}