using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
    public AsyncOperation asyncOperation;
    public Action action;

    public void LoadScene(Define.Scene type)
    {
        Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    public void LoadSceneWait(Define.Scene type)
    {
        Clear();
        asyncOperation = SceneManager.LoadSceneAsync(GetSceneName(type));
        asyncOperation.completed += (AsyncOperation obj) => OnLoadSceneCompleted(obj);
    }

    private void OnLoadSceneCompleted(AsyncOperation obj)
    {
        action.Invoke();
        action = null;
    }

    public void AddWaitEvent(Action action)
    {
        if (asyncOperation.isDone)
            action.Invoke();
        else
            this.action += action;
    }


    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
