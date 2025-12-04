using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NightEnemySpawner : MonoBehaviour
{
    public DayNightCycle cycle;
    public Spawner spawner;

    [Header("Night Spawn Settings")]
    public int baseEnemiesPerNight = 5;
    public float spawnInterval = 3f;

    List<GameObject> activeEnemies = new();
    bool spawning = false;

    void Start()
    {
        cycle.onSunset.AddListener(StartNight);
        cycle.onSunrise.AddListener(EndNight);
    }

    void StartNight()
    {
        int enemiesThisNight = baseEnemiesPerNight + (cycle.currentDay * 2);
        StartCoroutine(SpawnEnemies(enemiesThisNight));
    }

    IEnumerator SpawnEnemies(int count)
    {
        spawning = true;

        for (int i = 0; i < count; i++)
        {
            if (!cycle.isNight) break;

            GameObject enemy = spawner.SpawnEnemy();
            if (enemy != null)
                activeEnemies.Add(enemy);

            yield return new WaitForSeconds(spawnInterval);
        }

        spawning = false;
    }

    void EndNight()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                var member = enemy.GetComponent<PoolMember>();
                if (member != null)
                    member.pool.Release(enemy);
            }
        }

        activeEnemies.Clear();
    }
}
