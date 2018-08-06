using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates
{
    Null,
    Start,
    Playing,
    GameOver,
    End
}

public class TGBaseScene : MonoBehaviour
{
    protected float m_timeLeft;
    protected float m_gameOverTimeRemaining;
    protected float m_startTime;
    protected float m_timePassed;
    protected int m_score;
    protected int m_difficultyLv;
    protected GameStates m_gameState;
    protected int m_stageLevel = -1;

    public bool isActive = false;
    public TGUIRoot uiRoot;

    public int DifficultyLv

    {
        get { return m_difficultyLv; }
        set { OnDifficultyChanged(value); }
    }


    public float TimeLeft
    {
        get { return m_timeLeft; }
        set
        {
            m_timeLeft = Mathf.Clamp(value, 0, Duration);
            uiRoot.timeBar.SetValue(m_timeLeft / Duration);
        }
    }

    public float TimePassed
    {
        get { return m_timePassed; }
        set
        {
            float delta = value - m_timePassed;

            m_timePassed = value;

            TimeLeft -= delta;

            StageLevel = Mathf.FloorToInt(m_timePassed / 60);
        }
    }

    public int StageLevel
    {
        get { return m_stageLevel; }
        set
        {
            if (m_stageLevel != value)
            {
                OnStageLevelChanged(value);
            }
        }
    }

    public int Score
    {
        get { return m_score; }
        set
        {

            m_score = Mathf.Max(value, 0);
            uiRoot.scoreTxt.text = m_score.ToString();
        }
    }

    public GameStates GameState
    {
        get { return m_gameState; }
        set
        {

            if (m_gameState != value)
            {
                m_gameState = value;
                ConsoleProDebug.Watch("Game State", m_gameState.ToString());
                OnEnterGameState(m_gameState);
            }
        }
    }

    public string SceneName
    {
        get { return SceneManager.GetActiveScene().name; }
    }


    public TGController controller { get; private set; }
    public float Duration
    {
        get; private set;
    }

    public virtual void Init(TGController _controller)
    {
        uiRoot = FindObjectOfType<TGUIRoot>();

        controller = _controller;
        var config = controller.gameConfig.configInfo;

        Duration = config.trainingTime * 60;
        DifficultyLv = config.difficultyLv - 1;
    }

    public virtual void OnStart()
    {
        isActive = true;

        m_startTime = Time.time;

        Score = 0;
        TimeLeft = Duration;
        TimePassed = 0;

        GameState = GameStates.Start;
    }

    public virtual void GetOrLossScore(int _score, Vector3 _position)
    {
        Score += _score;

    }

    public virtual void LossScoreAndCreatePrefab(int _score, Vector3 _position)
    {
        Score += _score;

        var scnPos = Camera.main.WorldToScreenPoint(_position);
        uiRoot.CreateScorePrefab(_score, scnPos);
    }

    protected virtual void OnDifficultyChanged(int _difficulty)
    {
        m_difficultyLv = _difficulty;
    }

    public virtual void OnUpdate()
    {
        TimePassed += Time.deltaTime;

        if (m_gameState == GameStates.Playing)
            OnUpdateGamePlaying();

        if (m_gameState == GameStates.GameOver)
            OnUpdateGameOver();
    }

    public virtual void ExitScene()
    {
        isActive = false;
    }

    public virtual void OnEnterGameState(GameStates _gameState)
    {
        switch (_gameState)
        {
            case GameStates.Start:
                GameState = GameStates.Playing;
                break;

            case GameStates.Playing:
                OnStartPlaying();
                break;

            case GameStates.GameOver:
                GameOver();
                break;

            case GameStates.End:
                OnEnterGameEnd();
                break;

        }
    }

    public virtual void Restart()
    {
        SceneManager.LoadScene(0);
    }

    protected virtual void OnUpdateGamePlaying()
    {
        ConsoleProDebug.Watch("Time Left", TimeLeft.ToString());

        if (TimeLeft <= 0f)
        {
            GameState = GameStates.GameOver;
        }
    }
    protected virtual void OnUpdateGameOver()
    {
        m_gameOverTimeRemaining -= Time.deltaTime;

        uiRoot.gameOverPanel.SetCountdownTxt(Mathf.FloorToInt(m_gameOverTimeRemaining));

        if (m_gameOverTimeRemaining <= 0f)
        {
            GameState = GameStates.End;
        }
    }

    protected virtual void OnStageLevelChanged(int _level)
    {
        m_stageLevel = _level;

        Debug.Log("Level Chaged: " + m_stageLevel);
    }

    protected virtual void OnEnterGameEnd()
    {
        controller.gameConfig.configInfo.currentScore = Score;
        ExitScene();
    }

    protected virtual void OnStartPlaying() { }

    public virtual void GameOver()
    {
        uiRoot.gameOverPanel.Show(Score);
        m_gameOverTimeRemaining = 5f;
    }
}
