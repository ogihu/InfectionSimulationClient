using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    public Dictionary<string, Stack<GameObject>> UICache { get; set; } = new Dictionary<string, Stack<GameObject>>();
    Canvas canvas;

    /// <summary>
    /// Resources/Prefabs/UI 폴더 산하에 있는 UI를 생성
    /// 해당 UI의 name을 매개변수로 넘겨주면 됨
    /// 생성된 UI를 반환해주기 때문에 GameObject로 받아 해당 UI에 필요한 작업 수행 가능
    /// </summary>
    /// <param name="name"></param>
    /// <returns>GameObject</returns>
    public GameObject CreateUI(string name, Action action = null)
    {
        if (name == null)
            Debug.LogError("Can't find null key, check the key input");

        GameObject go;

        if (canvas == null)
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        if (UICache.ContainsKey(name))
        {
            Stack<GameObject> stack = UICache[name];
            
            if(stack.Count > 0)
            {
                go = stack.Pop();
                go.transform.parent = canvas.transform;
            }
            else
                go = Managers.Resource.Instantiate($"UI/{name}", canvas.transform);

            go.SetActive(true);

            if(action != null)
                action.Invoke();

            return go;
        }

        go = Managers.Resource.Instantiate($"UI/{name}", canvas.transform);
        UICache.Add(name, new Stack<GameObject>());
        go.SetActive(true);

        if (action != null)
            action.Invoke();

        return go;
    }

    public IEnumerator DestroyAfter(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        DestroyUI(go);
    }

    public void DestroyUI(GameObject go)
    {
        if (UICache.ContainsKey(go.name))
        {
            Stack<GameObject> stack = UICache[go.name];
            stack.Push(go);
            go.SetActive(false);
        }
        else
            UnityEngine.Object.Destroy(go);
    }

    public void Clear()
    {
        UICache.Clear();
    }
}
