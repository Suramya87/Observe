using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 20;
    public bool expandable = true;

    readonly Queue<GameObject> inactive = new();

    void Awake()
    {
        if (!prefab) { Debug.LogError("[Pool] Prefab not set", this); return; }
        for (int i = 0; i < initialSize; i++) Enqueue(NewInstance());
    }

    GameObject NewInstance()
    {
        var go = Instantiate(prefab, transform);
        go.SetActive(false);
        var member = go.GetComponent<PoolMember>() ?? go.AddComponent<PoolMember>();
        member.pool = this;
        return go;
    }

    void Enqueue(GameObject go)
    {
        go.SetActive(false);
        inactive.Enqueue(go);
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject go = (inactive.Count > 0) ? inactive.Dequeue()
                    : (expandable ? NewInstance() : null);
        if (!go) return null;
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);                        
        return go;
    }

    public void Release(GameObject go)
    {
        if (!go) return;
        Enqueue(go);                               
    }
}
