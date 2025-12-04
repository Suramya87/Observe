using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject dronePrefab;

    [Header("Spawn Settings")]  
    public float enemySpawnInterval = 3f;  
    private float enemyTimer;  

    [Header("Spawn Points")]  
    public Transform[] enemySpawnPoints;  
    public Transform[] droneSpawnPoints;

    [Header("TV Reference")]
    public TVScreenController tv;

    public ObjectPool enemyPool;  
    public ObjectPool dronePool;  
    public DayNightCycle cycle;  

    void Awake()  
    {  
        enemyPool = CreatePool(enemyPrefab, "EnemyPool");  
        dronePool = CreatePool(dronePrefab, "DronePool");  
    }  

    void Start()  
    {  
        if (cycle != null)  
        {  
            cycle.onSunrise.AddListener(KillAllEnemies);  
        }  
    }  

    // void Update()  
    // {  
    //     if (cycle != null && cycle.isNight)  
    //     {  
    //         HandleEnemySpawning();  
    //     }  
    // }  

    void HandleEnemySpawning()  
    {  
        enemyTimer += Time.deltaTime;  

        if (enemyTimer >= enemySpawnInterval)  
        {  
            enemyTimer = 0f;  
            SpawnEnemy();  
        }  
    }  

    public GameObject SpawnEnemy()  
    {  
        if (enemySpawnPoints.Length == 0) return null;  

        Transform point = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];  
        GameObject enemy = enemyPool.Get(point.position, point.rotation);  

        return enemy;  
    }  

    public GameObject SpawnDrone()  
    {  
        Transform[] activePoints =
            System.Array.FindAll(droneSpawnPoints, p => p != null && p.gameObject.activeInHierarchy);

        if (activePoints.Length == 0) return null;  

        Transform point = activePoints[Random.Range(0, activePoints.Length)];  
        GameObject drone = dronePool.Get(point.position, point.rotation);  

        return drone;  
    }  

    void KillAllEnemies()  
    {  
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  
        foreach (var enemy in enemies)  
        {  
            Destroy(enemy);  
        }  
    }  

    ObjectPool CreatePool(GameObject prefab, string poolName)  
    {  
        GameObject poolObj = new GameObject(poolName);  
        poolObj.transform.parent = this.transform;  

        ObjectPool pool = poolObj.AddComponent<ObjectPool>();  
        pool.prefab = prefab;  
        pool.initialSize = 20;  
        pool.expandable = true;  

        return pool;  
    }  

    public GameObject SpawnDroneForCamera(int cameraIndex)
    {
        if (droneSpawnPoints.Length == 0) return null;

        // Ensure index is valid
        if (cameraIndex < 0 || cameraIndex >= droneSpawnPoints.Length) return null;

        Transform point = droneSpawnPoints[cameraIndex];
        if (point == null) return null;

        GameObject drone = dronePool.Get(point.position, point.rotation);
        return drone;
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (tv != null)
            {
                SpawnDroneForCamera(tv.ActiveIndex);
            }
        }

        if (cycle != null && cycle.isNight)
        {
            HandleEnemySpawning();
        }
    }
}

