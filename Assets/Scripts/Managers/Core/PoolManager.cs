using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PoolManager
{
	#region Pool
	class Pool
    {
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }

        Stack<Poolable> _poolStack = new Stack<Poolable>();

        //풀 오브젝트를 생성할 때, 최소 갯수로 생성하여 초기화
        public void Init(GameObject original, int count = 3)
        {
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (int i = 0; i < count; i++)
                Push(Create());
        }

        //Original에 해당하는 게임오브젝트를 생성하고, Poolable 컴포넌트를 추가, 반환
        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        //Pool이 가능한 오브젝트일 경우, Root의 산하에 보관하고, 스택에 추가
        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            poolable.transform.SetParent(Root, false);
            
            if(poolable.gameObject.layer != LayerMask.NameToLayer("UI"))
            {
                poolable.transform.position = Vector3.zero;
            }
            poolable.gameObject.SetActive(false);
            poolable.IsUsing = false;

            _poolStack.Push(poolable);
        }

        //Pool에 남은 오브젝트가 있는 경우, 해당 오브젝트를 가져오고 없다면 생성하여 반환
        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.gameObject.SetActive(true);

            // DontDestroyOnLoad 해제 용도
            if (parent == null)
                poolable.transform.parent = Managers.Scene.CurrentScene.transform;

            poolable.transform.SetParent(parent, false);
            poolable.IsUsing = true;

            return poolable;
        }

        public Poolable Pop(Vector3 position, Quaternion rotation, Transform parent)
        {
            Poolable poolable;

            if (_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();

            poolable.gameObject.SetActive(true);

            // DontDestroyOnLoad 해제 용도
            if (parent == null)
                poolable.transform.parent = Managers.Scene.CurrentScene.transform;

            poolable.transform.parent = parent;
            poolable.transform.position = position;
            poolable.transform.rotation = rotation;
            poolable.IsUsing = true;

            return poolable;
        }
    }
	#endregion

	Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    Transform _root;

    //전체 Pool을 관리하는 Root를 생성하여 삭제되지 않도록 관리
    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            Object.DontDestroyOnLoad(_root);
        }
    }

    //Pool오브젝트를 생성
    public void CreatePool(GameObject original, int count = 1)
    {
        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.parent = _root;

        _pool.Add(original.name, pool);
    }

    //Pool이 가능한 오브젝트만 Pool에 반환
    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }

        _pool[name].Push(poolable);
    }

    public async void Push(Poolable poolable, float delay)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject, delay);
            return;
        }

        await Task.Delay((int)(delay * 1000));
        _pool[name].Push(poolable);
    }

    //필요한 오브젝트를 Pool에서 가져와 반환
    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
            CreatePool(original);

        return _pool[original.name].Pop(parent);
    }

    public Poolable Pop(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
            CreatePool(original);

        return _pool[original.name].Pop(position, rotation, parent);
    }

    //name에 해당하는 오브젝트를 찾아 반환
    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;
        return _pool[name].Original;
    }

    //Pool을 정리
    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);

        _pool.Clear();
    }
}
