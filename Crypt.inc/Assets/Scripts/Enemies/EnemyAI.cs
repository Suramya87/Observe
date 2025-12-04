using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    private PlayerHealth playerHealth;

    [Header("Detection Settings")]
    public float sightRange = 15f;

    [Header("Attack Settings")]
    public int damageAmount = 10;        // how much damage to deal
    public float attackCooldown = 1f;    // seconds between hits

    private bool playerInSightRange;
    private float lastAttackTime;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }

        // Make sure the collider is set up for triggering
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (player == null || agent == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = distanceToPlayer <= sightRange;

        if (playerInSightRange)
        {
            ChasePlayer();
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void ChasePlayer()
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Check if we are touching the player
        if (other.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            if (playerHealth == null)
                playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
