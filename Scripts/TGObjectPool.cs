using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TGObjectPool : MonoBehaviour
{
    public Component[] poolObjects;
    public Component targetPrefab;

    private int m_curIndex;

    private static Dictionary<int, TGObjectPool> poolDict = new Dictionary<int, TGObjectPool>();

    public static TGObjectPool Create(Component prefab, int poolLength)
    {
        int id = prefab.GetInstanceID();
        Debug.Log("Create a new Pool with prefab: " + id);
        var pool = new GameObject(prefab.name + " Pool").AddComponent<TGObjectPool>();
        pool.Init(prefab, poolLength);
        poolDict.Add(id, pool);
        return pool;
    }

    public static T Instantiate<T>(T prefab, int poolLength = 100) where T : Component
    {
        return Instantiate(prefab, Vector3.zero, poolLength);
    }

    public static T Instantiate<T>(T prefab, Vector3 pos, int poolLength = 100) where T : Component
    {
        return Instantiate(prefab, pos, Quaternion.identity, poolLength);
    }

    public static T Instantiate<T>(T prefab, Vector3 pos, Quaternion rotation, int poolLength = 100) where T : Component
    {
        var pool = GetOrCreatePool(prefab, poolLength);
        return pool.Instantiate<T>(pos, rotation);
    }

    public static void Destroy(GameObject go)
    {
        go.SetActive(false);
    }

    private static TGObjectPool GetOrCreatePool<T>(T prefab, int length) where T : Component
    {
        int id = prefab.GetInstanceID();
        if (poolDict.ContainsKey(id))
        {
            return poolDict[id];
        }

        return Create(prefab, length);
    }

    public void Init(Component prefab, int poolLength)
    {
        poolObjects = new Component[poolLength];
        targetPrefab = prefab;
    }

    public T Instantiate<T>(Vector3 pos, Quaternion rotation) where T : Component
    {
        if (m_curIndex >= poolObjects.Length)
            return ActiveRestObject<T>(pos, rotation);
        else
            return CreateNewObject<T>(pos, rotation);
    }

    private T ActiveRestObject<T>(Vector3 pos, Quaternion rotation) where T : Component
    {
        var rest = poolObjects.FirstOrDefault(obj => !obj.gameObject.activeSelf);
        if (rest == null)
        {
            Debug.LogWarning("The activatable object is out of the pool!");
            return null;
        }

        rest.transform.position = pos;
        rest.transform.rotation = rotation;

        return rest as T;
    }

    private T CreateNewObject<T>(Vector3 pos, Quaternion rotation) where T : Component
    {
        var retval = Instantiate<T>(targetPrefab as T, pos, rotation);
        retval.transform.SetParent(transform);

        poolObjects[m_curIndex] = retval;
        m_curIndex++;

        return retval;
    }
}