using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IPoolObject
{
    bool IsActive { get; }
    string PoolName { get; }
    void Spawn(Vector3 position, Quaternion rotation);
}

public class TGObjectPool : MonoBehaviour
{

    private static Dictionary<int, TGObjectPool> poolDict = new Dictionary<int, TGObjectPool>();
    private const string OUT_OF_POOL_ERROR = "Out of pool";

    public static TGObjectPool CreatePool(IPoolObject prefab, int poolLength)
    {
        int id = prefab.GetHashCode();
        Debug.Log("Create a new Pool with prefab: " + id);
        var pool = new GameObject("[Pool]::" + prefab.PoolName).AddComponent<TGObjectPool>();
        pool.Init(prefab, poolLength);
        poolDict.Add(id, pool);
        return pool;
    }

    public static IPoolObject Instantiate(IPoolObject prefab, Vector3 pos, Quaternion rotation, int poolLength = 100)
    {
        var pool = GetOrCreatePool(prefab, poolLength);
        return pool.CreateOrActiveObject<IPoolObject>(pos, rotation);
    }

    public static T Instantiate<T>(T prefab, int poolLength = 100)where T : IPoolObject
    {
        return Instantiate(prefab, Vector3.zero, poolLength);
    }

    public static T Instantiate<T>(T prefab, Vector3 pos, int poolLength = 100)where T : IPoolObject
    {
        return Instantiate(prefab, pos, Quaternion.identity, poolLength);
    }

    public static T Instantiate<T>(T prefab, Vector3 pos, Quaternion rotation, int poolLength = 100)where T : IPoolObject
    {
        var pool = GetOrCreatePool(prefab, poolLength);
        return pool.CreateOrActiveObject<T>(pos, rotation);
    }

    private static TGObjectPool GetOrCreatePool<T>(T prefab, int length)where T : IPoolObject
    {
        int id = prefab.GetHashCode();
        if (poolDict.ContainsKey(id))
        {
            return poolDict[id];
        }

        return CreatePool(prefab, length);
    }

    public IPoolObject[] poolObjects;
    public IPoolObject targetPrefab;

    private int m_curIndex;

    public void Init(IPoolObject prefab, int poolLength)
    {
        poolObjects = new IPoolObject[poolLength];
        targetPrefab = prefab;
    }

    public T CreateOrActiveObject<T>(Vector3 pos, Quaternion rotation)where T : IPoolObject
    {
        var obj = ActiveRestObject<T>(pos, rotation);

        if (obj == null)
            obj = CreateNewObject<T>(pos, rotation);

        return obj;
    }

    private T ActiveRestObject<T>(Vector3 pos, Quaternion rotation)where T : IPoolObject
    {
        var rest = poolObjects.FirstOrDefault(obj => obj != default(IPoolObject) && !obj.IsActive);

        if (rest == null)
        {
            Debug.LogWarning("The activatable object is out of the pool!");
            return default(T);
        }

        ((Component)rest).gameObject.SetActive(true);
        rest.Spawn(pos, rotation);

        return (T)rest;
    }

    private T CreateNewObject<T>(Vector3 pos, Quaternion rotation)where T : IPoolObject
    {
        if (m_curIndex >= poolObjects.Length)
        {
            Debug.LogWarning(name + " index is over the pool length " + poolObjects.Length + "!");
            return default(T);
        }

        var comp = GameObject.Instantiate((Component)targetPrefab);
        comp.transform.SetParent(transform);

        var retval = (IPoolObject)comp;
        retval.Spawn(pos, rotation);

        poolObjects[m_curIndex] = retval;
        m_curIndex++;

        return (T)retval;
    }
}