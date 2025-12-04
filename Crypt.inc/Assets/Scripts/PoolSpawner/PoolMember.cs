using UnityEngine;

public class PoolMember : MonoBehaviour
{
    [HideInInspector] public ObjectPool pool;
    public void ReturnToPool() => pool?.Release(gameObject);
}
