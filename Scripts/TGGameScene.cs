using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGGameScene : TGBaseScene
{
    protected float m_timeLeft;
    protected float m_gameOverTimeRemaining;
    protected int m_score;
    protected int m_difficultyLv;
    protected GameStates m_gameState;
    protected int m_stageLevel = -1;

    public Sound bgm;
    public TGUIRoot uiRoot {get; private set;}

    public int DifficultyLv

    {
        get { return m_difficultyLv; }
        set { OnDifficultyChanged(value); }
    }

    public float Duration
    {
        get; private set;
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

    protected override void OnTimePassed(float _value)
    {
		float delta = _value - m_timePassed;
        base.OnTimePassed(_value);
        TimeLeft -= delta;
        StageLevel = Mathf.FloorToInt(m_timePassed / 60);
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

    public override void Init(TGController _controller)
    {
        base.Init(_controller);

        uiRoot = FindObjectOfType<TGUIRoot>();
        uiRoot.exitBtn.onClick.AddListener(() => GameState = GameStates.FreezeToExitGame);
        uiRoot.recalibrationBtn.onClick.AddListener(Recalibration);

        uiRoot.exitGamePanel.confirmBtn.onClick.AddListener(ExitScene);
        uiRoot.exitGamePanel.cancelBtn.onClick.AddListener(uiRoot.exitGamePanel.Exit);
        uiRoot.exitGamePanel.onFinishClosePanel += () => GameState = GameStates.Playing;

        var config = controller.gameConfig;

        Duration = config.GetValue("训练时长", -1) * 60;
        DifficultyLv = config.GetValue("难度等级", -1);
    }

    public override void OnStart()
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

        var scnPos = Camera.main.WorldToScreenPoint(_position);
        uiRoot.CreateScorePrefab(_score, scnPos);
    }

    protected virtual void OnDifficultyChanged(int _difficulty)
    {
        m_difficultyLv = _difficulty;
    }

    public override void OnUpdate()
    {
		base.OnUpdate();

        if (m_gameState == GameStates.Playing)
            OnUpdateGamePlaying();

        if (m_gameState == GameStates.GameOver)
            OnUpdateGameOver();
    }

    public virtual void OnEnterGameState(GameStates _gameState)
    {
        switch (_gameState)
        {
            case GameStates.Start:
                if (bgm.clip != null)
                    AudioMng.Instance.Play(bgm);

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

            case GameStates.FreezeToExitGame:
                OnFreezeToExitGame();
                break;

        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    protected virtual void OnFreezeToExitGame()
    {
        Time.timeScale = 0f;
        uiRoot.exitGamePanel.Show();
    }

    protected virtual void OnUpdateGamePlaying()
    {
        if (TimeLeft <= 0f)
        {
            GameState = GameStates.GameOver;
        }
    }

    protected virtual void OnUpdateGameOver()
    {
        if (bgm.clip != null)
            AudioMng.Instance.Fade(bgm, 0f, 1f);

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
        ExitScene();
    }

	public override void ExitScene()
	{
		AudioMng.Instance.StopAll();

        additionDataToSave.Add("分数", Score.ToString());
		base.ExitScene();
	}

    protected virtual void OnStartPlaying()
    {
        Time.timeScale = 1f;
    }

    public virtual void GameOver()
    {
        uiRoot.gameOverPanel.Show(Score);
        m_gameOverTimeRemaining = 5f;
        StartCoroutine(CaptureDelay());
    }

    IEnumerator CaptureDelay()
    {
        yield return new WaitForSeconds(1f);
        CaptureScreen();
    }

}


