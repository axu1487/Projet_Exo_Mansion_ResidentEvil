using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Attack, Dead }
    public AIState currentState;

    [Header("Références")]
    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;

    [Header("Patrouille")]
    public List<Transform> patrolPoints;
    private int currentPatrolIndex = 0;
    public bool randomPatrol = false;
    public float idleTime = 2f;
    private float idleTimer;
    private bool isWaiting = false;
    public float turnSpeed = 300f;
    private bool isTurning = false;

    [Header("Détection & Combat")]
    public float sightDistance = 10f;
    public float attackDistance = 2f;
    public float attackRate = 1.5f;
    private float nextAttackTime = 0f;
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Noms des paramètres Animator")]
    public string speedParamName = "SpeedZ";
    public string attackTriggerName = "Attack";
    public string hitTriggerName = "Hit";
    public string dieTriggerName = "Die";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
        currentState = AIState.Patrol;
        
        if (patrolPoints.Count > 0)
            PrepareNextPatrolPoint();
    }

    void Update()
    {
        if (currentState == AIState.Dead || player == null) return;

        UpdateAnimator();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Patrol:
                PatrolLogic(distanceToPlayer);
                break;
            case AIState.Chase:
                ChaseLogic(distanceToPlayer);
                break;
            case AIState.Attack:
                AttackLogic(distanceToPlayer);
                break;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat(speedParamName, currentSpeed);
    }

    void PatrolLogic(float distanceToPlayer)
    {
        if (distanceToPlayer <= sightDistance && IsPlayerOnNavMesh())
        {
            isTurning = false;
            agent.isStopped = false;
            currentState = AIState.Chase;
            return;
        }

        if (isTurning)
        {
            RotateTowardsNextPoint();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                idleTimer = idleTime;
            }

            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                PrepareNextPatrolPoint();
                isWaiting = false;
            }
        }
    }

    void ChaseLogic(float distanceToPlayer)
    {
        if (!IsPlayerOnNavMesh())
        {
            currentState = AIState.Patrol;
            PrepareNextPatrolPoint();
            return;
        }

        if (distanceToPlayer <= attackDistance)
        {
            currentState = AIState.Attack;
            agent.ResetPath();
            return;
        }

        agent.SetDestination(player.position);
    }

    void AttackLogic(float distanceToPlayer)
    {
        if (distanceToPlayer > attackDistance && distanceToPlayer <= sightDistance)
        {
            currentState = AIState.Chase;
            return;
        }
        else if (distanceToPlayer > sightDistance || !IsPlayerOnNavMesh())
        {
            currentState = AIState.Patrol;
            PrepareNextPatrolPoint();
            return;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (Time.time >= nextAttackTime)
        {
            if (animator != null)
            {
                animator.SetTrigger(attackTriggerName);
            }
            nextAttackTime = Time.time + attackRate;
        }
    }

    void PrepareNextPatrolPoint()
    {
        if (patrolPoints.Count == 0) return;

        if (randomPatrol)
        {
            currentPatrolIndex = Random.Range(0, patrolPoints.Count);
        }
        else
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }

        agent.isStopped = true;
        isTurning = true;
    }

    void RotateTowardsNextPoint()
    {
        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;
        Vector3 direction = (targetPoint - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
            {
                isTurning = false;
                agent.isStopped = false;
                agent.SetDestination(targetPoint);
            }
        }
    }

    bool IsPlayerOnNavMesh()
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(player.position, out hit, 1.0f, NavMesh.AllAreas);
    }

    public void TakeDamage(int damage)
    {
        if (currentState == AIState.Dead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Déclenche l'animation de réaction aux dégâts
            if (animator != null)
            {
                animator.SetTrigger(hitTriggerName);
            }

            if (currentState == AIState.Patrol)
            {
                currentState = AIState.Chase;
            }
        }
    }

    void Die()
    {
        currentState = AIState.Dead;

        // Déclenche l'animation de mort
        if (animator != null)
        {
            animator.SetTrigger(dieTriggerName);
        }

        // Désactive le NavMeshAgent
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Désactive les collisions pour ne pas bloquer le joueur ou les balles
        Collider col = GetComponent<Collider>();
        if (col != null) 
        {
            col.enabled = false;
        }

        Debug.Log("L'ennemi est mort.");
    }
}