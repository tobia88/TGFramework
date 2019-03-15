using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates
{
    Null,
    Tutorial,
    Start,
    Playing,
    FreezeToExitGame,
    GameOver,
    End
}

public enum GameTypes
{
    TimeLimit,
    Missions
}

public class TGGameScene : TGBaseScene
{
    protected float m_timeLeft;
    protected int m_score;
    protected int m_difficultyLv;
    protected GameStates m_gameState;
    protected int m_stageLevel = -1;

    public Sound bgm;
    public TGUIRoot uiRoot;
    public GameTypes gameType;

    public int DifficultyLv

    {
        get { return m_difficultyLv; }
        set { OnDifficultyChanged(value); }
    }

    public float Duration
    {
        get;
        private set;
    }

    public float TimeLeft
    {
        get { return m_timeLeft; }
        set
        {
            if (gameType != GameTypes.TimeLimit)
                return;

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
                OnEnterGameState(m_gameState);
            }
        }
    }

    public override void Init()
    {
        base.Init();

        uiRoot.gameObject.SetActive(true);
        uiRoot.exitBtn.onClick.AddListener(() => GameState = GameStates.FreezeToExitGame);
        uiRoot.recalibrationBtn.onClick.AddListener(Recalibration);
        uiRoot.questionBtn.onClick.AddListener(() => GameState = GameStates.Tutorial);

        uiRoot.exitGamePanel.confirmBtn.onClick.AddListener(ExitScene);
        uiRoot.exitGamePanel.cancelBtn.onClick.AddListener(uiRoot.exitGamePanel.Exit);
        uiRoot.exitGamePanel.onFinishClosePanel += () => GameState = GameStates.Playing;

        uiRoot.tutorialPanel.SetImage(Resources.Load<Sprite>(controller.inputSetting.DeviceName));
        uiRoot.tutorialPanel.confirmBtn.onClick.AddListener(uiRoot.tutorialPanel.Exit);
        uiRoot.tutorialPanel.onFinishClosePanel += () => GameState = GameStates.Playing;

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

    public GetPointTextUI GetOrLossScore(int _score, Vector3 _position, bool _isScreenPos = false)
    {
        Score += _score;

        var scnPos = (_isScreenPos) ? _position : Camera.main.WorldToScreenPoint(_position);
        return uiRoot.CreateScorePrefab(_score, scnPos);
    }

    protected virtual void OnDifficultyChanged(int _difficulty)
    {
        m_difficultyLv = _difficulty;
        controller.inputSetting.SetPressureLevel(m_difficultyLv);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (m_gameState == GameStates.Playing)
            OnUpdateGamePlaying();
    }

    public virtual void OnEnterGameState(GameStates _gameState)
    {
        switch (_gameState)
        {
            case GameStates.Tutorial:
                OnStartTutorial();
                break;

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

    protected virtual void OnStartTutorial()
    {
        Time.timeScale = 0f;
        uiRoot.tutorialPanel.Show();
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
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Q))
        {
            TimeLeft = 0f;
            GameState = GameStates.GameOver;
        }

        if (gameType == GameTypes.TimeLimit)
        {
            if (TimeLeft <= 0f)
            {
                GameState = GameStates.GameOver;
            }
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

        extraData.Add("分数", Score.ToString());
        base.ExitScene();
    }

    protected virtual void OnStartPlaying()
    {
        Time.timeScale = 1f;
    }

    public virtual void GameOver()
    {
        if (bgm.clip != null)
            AudioMng.Instance.Fade(bgm, 0f, 1f);

        uiRoot.gameOverPanel.SetScore(Score);
        uiRoot.gameOverPanel.Show();

        Debug.Log("Game Over");

        StartCoroutine(CountdownToQuitRoutine());
        // StartCoroutine(CaptureDelay());
    }

    IEnumerator CountdownToQuitRoutine()
    {
        for (int i = 5; i > 0; i--)
        {
            uiRoot.gameOverPanel.SetCountdownTxt(i);
            yield return new WaitForSeconds(1);
        }

        uiRoot.gameOverPanel.SetCountdownTxt(0);

        var dateStr = controller.endTime.ToString("yyyy_MM_dd_HH_mm_ss");

        yield return StartCoroutine(RecordFrame(dateStr));

        GameState = GameStates.End;
    }

    // IEnumerator CaptureDelay()
    // {
    //     yield return new WaitForSeconds(1f);
    //     CaptureScreen();
    // }

}