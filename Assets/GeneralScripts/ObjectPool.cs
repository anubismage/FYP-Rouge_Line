using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    public GameObject prefab;
    public List<GameObject> pooledObjects;
    public int countToPool = 20;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        pooledObjects = new List<GameObject>();
        for(int i =0; i < countToPool; i++)
        {
            GameObject obj = Instantiate(prefab);
            prefab.name = "Bullet "+ i;
            obj.SetActive(false);
            pooledObjects.Add(obj);

        }
    }

    public GameObject GetPooledObject()
    {
        for(int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}
