using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    private readonly Dictionary<int, List<Object>> poolDictionary = new Dictionary<int, List<Object>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void NewPool(GameObject prefab, int size)
    {
        if (prefab == null) return;
        int id = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(id)) return;

        var newPool = new List<UnityEngine.Object>(size);
        for (int i = 0; i < size; i++)
        {
            newPool.Add(CreateNewGameObject(prefab));
        }
        poolDictionary.Add(id, newPool);
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        int id = prefab.GetInstanceID();

        if (!poolDictionary.TryGetValue(id, out var pool))
        {
            NewPool(prefab, 4);
            pool = poolDictionary[id];
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null) continue;
            GameObject go = (GameObject)pool[i];

            if (!go.activeSelf)
            {
                go.transform.SetPositionAndRotation(position, rotation);

                go.SetActive(true);
                return go;
            }
        }

        GameObject newGo = CreateNewGameObject(prefab);
        newGo.transform.SetPositionAndRotation(position, rotation);

        if (newGo.TryGetComponent<TrailRenderer>(out var newTrail)) newTrail.Clear();

        newGo.SetActive(true);
        pool.Add(newGo);
        return newGo;
    }

    public void NewPool<T>(T prefab, int size) where T : Component
    {
        if (prefab == null) return;
        int id = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(id)) return;

        var newPool = new List<Object>(size);
        for (int i = 0; i < size; i++)
        {
            newPool.Add(CreateNewComponent(prefab));
        }
        poolDictionary.Add(id, newPool);
    }

    public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefab == null) return null;
        int id = prefab.GetInstanceID();

        if (!poolDictionary.TryGetValue(id, out var pool))
        {
            NewPool(prefab, 4);
            pool = poolDictionary[id];
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null) continue;
            T comp = (T)pool[i];

            if (!comp.gameObject.activeSelf)
            {
                comp.transform.SetPositionAndRotation(position, rotation);

                if (comp is TrailRenderer trail) trail.Clear();

                comp.gameObject.SetActive(true);
                return comp;
            }
        }

        T newComp = CreateNewComponent(prefab);
        newComp.transform.SetPositionAndRotation(position, rotation);

        if (newComp is TrailRenderer newTrail) newTrail.Clear();

        newComp.gameObject.SetActive(true);
        pool.Add(newComp);
        return newComp;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
    }

    public void Return<T>(T component) where T : Component
    {
        if (component == null) return;
        component.gameObject.SetActive(false);
    }

    private GameObject CreateNewGameObject(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, transform);
        go.SetActive(false);
        return go;
    }

    private T CreateNewComponent<T>(T prefab) where T : Component
    {
        T comp = Instantiate(prefab, transform);
        comp.gameObject.SetActive(false);
        return comp;
    }
}