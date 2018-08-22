using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameStates
{
    Null,
    Start,
    Playing,
    FreezeToExitGame,
    GameOver,
    End
}

public class TGBaseScene : MonoBehaviour
{
    protected float m_startTime;
    protected float m_timePassed;

    public TGController controller { get; private set; }

    public float TimePassed
    {
        get { return m_timePassed; }
        set
        {
            OnTimePassed(value);
        }
    }

    public bool isActive = false;

    public virtual void Init(TGController _controller) 
    {
        controller = _controller;
    }

    public virtual void OnStart()
    {
        isActive = true;

        m_startTime = Time.time;

        TimePassed = 0;
    }

    public virtual void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            ExitScene();
            return;
        }

        TimePassed += Time.deltaTime;
    }

    public virtual void ExitScene()
    {
        isActive = false;
    }

    public void DelayCall(System.Action _func, float _delay)
    {
        StartCoroutine(DelayCallRoutine(_func, _delay));
    }

    protected virtual void OnTimePassed(float _value)
    {
        m_timePassed = _value;
    }

    IEnumerator DelayCallRoutine(System.Action _func, float _delay)
    {
        yield return new WaitForSeconds(_delay);
        _func();
    }
}
