using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectPoolItem
{
    public int _amountToPool;
    public GameObject _objectToPool;
}
public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling SharedInstance;
    public List<ObjectPoolItem> itemsToPool;
    private List<GameObject> pooledObjects;

    void Awake()
    {
        SharedInstance = this;
    }

    // Use this for initialization
    void Start()
    {
        pooledObjects = new List<GameObject>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 1; i < item._amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item._objectToPool);
                obj.SetActive(true);
                pooledObjects.Add(obj);
            }
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        for (int i = 4; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy && (pooledObjects[i].tag == tag))
            {
                return pooledObjects[i];
            }
        }
        foreach (ObjectPoolItem item in itemsToPool)
        {
            if (item._objectToPool.tag == tag)
            {
                GameObject obj = (GameObject)Instantiate(item._objectToPool);
                obj.SetActive(true);
                pooledObjects.Add(obj);
                return obj;
            }
        }
        return null;
    }
}
