using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FINISH SCRIPT ONCE AGENT POOL AND ITEM POOL CREATED
public class ObjectPool : MonoBehaviour {
    public List<GameObject> PooledObjects;
    public GameObject ObjectToPool;
    public int AmountToPool;

    // Start is called before the first frame update
    void Start() {
        PooledObjects = new List<GameObject>();
        GameObject tmp;

        for(var i = 0; i < AmountToPool; i++) {
            tmp = Instantiate(ObjectToPool);
            tmp.SetActive(false);
            PooledObjects.Add(tmp);
        }
    }

    // Get object to be pooled
    public GameObject GetPooledObject() {
        for(var i = 0; i < AmountToPool; i++) {
            if (!PooledObjects[i].activeInHierarchy)
                return PooledObjects[i];
        }

        return null;
    }
}
