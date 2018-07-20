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
    protected int m_score;
    protected GameStates m_gameState;

    public bool isActive = false;
    public AutoGameOverPanel gameOverPanel;
    public Transform gameplayPanel;
    public TMPro.TextMeshProUGUI scoreTxt;
    public TimeBar TimeBar;
    public GetPointTextUI getScorePrefab;
    public GetPointTextUI lossScorePrefab;

    public float TimeLeft
    {
        get { return m_timeLeft; }
        set
        {
            m_timeLeft = Mathf.Clamp(value, 0, Duration);
            TimeBar.SetValue(m_timeLeft / Duration);
        }
    }
    public int Score
    {
        get { return m_score; }
        set
        {
            m_score = Mathf.Max(value, 0);
            scoreTxt.text = m_score.ToString();
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
        controller = _controller;
        var config = controller.gameConfig.configInfo;

        Duration = config.trainingTime * 60;
    }

    public virtual void OnStart()
    {
        isActive = true;

        Score = 0;
        TimeLeft = Duration;

        GameState = GameStates.Start;
    }

    public virtual void AddScoreAndCreatePrefab(int _score, Vector3 _position)
    {
        Score += _score;

        var score = Instantiate(getScorePrefab);
        score.transform.SetParent(gameplayPanel, false);
        score.transform.position = Camera.main.WorldToScreenPoint(_position);
        score.SetText("+" + _score);
    }

    public virtual void LossScoreAndCreatePrefab(int _score, Vector3 _position)
    {
        Score += _score;

        var score = Instantiate(lossScorePrefab);
        score.transform.SetParent(gameplayPanel, false);
        score.transform.position = Camera.main.WorldToScreenPoint(_position);
        score.SetText(_score.ToString());
    }

    public virtual void OnUpdate()
    {
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

        TimeLeft -= Time.deltaTime;

        if (TimeLeft <= 0f)
        {
            GameState = GameStates.GameOver;
        }
    }
    protected virtual void OnUpdateGameOver()
    {
        m_gameOverTimeRemaining -= Time.deltaTime;

        gameOverPanel.SetCountdownTxt(Mathf.FloorToInt(m_gameOverTimeRemaining));

        if (Time.time - m_gameOverTimeRemaining >= 5f)
        {
            GameState = GameStates.End;
        }
    }
    protected virtual void OnEnterGameEnd()
    {
        controller.gameConfig.configInfo.currentScore = Score;
        ExitScene();
    }

    protected virtual void OnStartPlaying() { }

    public virtual void GameOver()
    {
        gameOverPanel.Show(Score);
        m_gameOverTimeRemaining = 5f;
    }
}
