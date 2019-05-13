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
    protected float m_gameTimePassed;
    protected int m_score;
    protected int m_difficultyLv;
    protected GameStates m_gameState;
    protected int m_stageLevel = -1;

    public Sound bgm;
    public TGUIRoot uiRoot;
    public GameTypes gameType;
    public bool showScoreTxt = true;

    // game.txt里的难度等级设置
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
            // 如果游戏类型是限时类型
            // 则不设置剩余时间
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

    // 当前关卡的阶段性难度等级
    // 现阶段被设置成了1分钟以内为等级1，2分钟以内为等级2...以此类推
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
            OnScoreChanged(value);
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

        InitUI();

        var config = controller.gameConfig;

        Duration = config.GetValue("训练时长", -1) * 60;
        DifficultyLv = config.GetValue("难度等级", -1);
    }

    private void InitUI()
    {
        // 根据设备名称获取教程图片，请确保Resources文件夹下的与keyInputConfig.json
        // 下的设备名称保持一致
        var tutorialSpr = Resources.Load<Sprite>(controller.inputSetting.DeviceName);

        uiRoot.Init(this, tutorialSpr);

        // 如果成功获取教程图片，则绑定点击事件和教程弹出窗口
        if (tutorialSpr != null)
        {
            uiRoot.questionBtn.onClick.AddListener(() => GameState = GameStates.Tutorial);
            uiRoot.tutorialPanel.confirmBtn.onClick.AddListener(uiRoot.tutorialPanel.Exit);
            uiRoot.tutorialPanel.onFinishClosePanel += () => GameState = GameStates.Playing;
        }

        // 根据keyInputConfig.json里的设置来确定是否要开启校准按钮
        var disableRecalibrate = controller.inputSetting.KeyportData.disableRecalibrate;

        uiRoot.recalibrationBtn.gameObject.SetActive(!disableRecalibrate);

        // 如果确定开启校准按钮，则绑定点击事件
        if (!disableRecalibrate)
            uiRoot.recalibrationBtn.onClick.AddListener(Recalibration);


        // 右上角退出按钮的点击事件绑定
        uiRoot.exitBtn.onClick.AddListener(OnPressExitGame);

        // 退出游戏的弹出窗口事件绑定
        uiRoot.exitGamePanel.confirmBtn.onClick.AddListener(ExitScene);
        uiRoot.exitGamePanel.cancelBtn.onClick.AddListener(uiRoot.exitGamePanel.Exit);
        uiRoot.exitGamePanel.onFinishClosePanel += () => GameState = GameStates.Playing;

        // 根据showScoreTxt选项确定是否显示左上角的分数
        uiRoot.scoreTxt.gameObject.SetActive(showScoreTxt);
    }

    protected override void OnPressExit()
    {
        OnPressExitGame();
    }

    protected virtual void OnScoreChanged(int value)
    {
        m_score = Mathf.Max(value, 0);

        if (uiRoot.scoreTxt != null && uiRoot.scoreTxt.isActiveAndEnabled)
            uiRoot.scoreTxt.text = m_score.ToString();
    }

    private void OnPressExitGame()
    {
        if (!isActive)
            return;

        TimeLeft = 0f;
        if (m_timePassed <= 15f)
            ExitScene();
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

    //  退出游戏弹窗
    protected virtual void OnFreezeToExitGame()
    {
        Time.timeScale = 0f;
        uiRoot.exitGamePanel.Show();
    }

    protected virtual void OnUpdateGamePlaying()
    {
        #if Unity_Editor
        // 一些方便测试的快捷键
        // LShift + Q直接结束游戏
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Q))
        {
            TimeLeft = 0f;
            GameState = GameStates.GameOver;
        }

        // =键加10分
        if (Input.GetKey(KeyCode.Equals))
        {
            Score += 10;
        }
        #endif

        m_gameTimePassed += Time.deltaTime;

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

        uiRoot.gameOverPanel.SetScore(Score.ToString());
        uiRoot.gameOverPanel.Show();

        Debug.Log("Game Over");

        StartCoroutine(CountdownToQuitRoutine());
    }

    IEnumerator CountdownToQuitRoutine()
    {
        for (int i = 5; i > 0; i--)
        {
            uiRoot.gameOverPanel.SetCountdownTxt(i);
            yield return new WaitForSeconds(1);
        }

        uiRoot.gameOverPanel.SetCountdownTxt(0);
        
        var now = System.DateTime.Now;
        var dateStr = now.ToString("yyyy_MM_dd_HH_mm_ss");
        controller.endTime = now;

        yield return StartCoroutine(RecordFrame(dateStr));

        GameState = GameStates.End;
    }
}