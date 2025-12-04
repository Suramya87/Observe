using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PoolMember))]
public class PooledAutoReturn : MonoBehaviour
{
    public float lifeSeconds = 6f;
    PoolMember member;
    Coroutine lifeCo;

    void Awake() { member = GetComponent<PoolMember>(); }

    void OnEnable()
    {
        if (lifeSeconds > 0f)
            lifeCo = StartCoroutine(Life());
    }

    void OnDisable()
    {
        if (lifeCo != null) StopCoroutine(lifeCo);
        lifeCo = null;
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(lifeSeconds);
        member.ReturnToPool();
    }

    void OnCollisionEnter(Collision _) => member.ReturnToPool();  
    void OnTriggerEnter(Collider _) => member.ReturnToPool();   
}
