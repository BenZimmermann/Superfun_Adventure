using UnityEngine;
using System;
using Unity.Collections;
public class BaseEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] public EnemyData enemyData;
    [SerializeField] private LayerMask playerLayer;

    private int currentHealth;
    private int damageAmount;

    public event Action OnDeath;

    private IDamageable player;
    public int CurrentHealth => currentHealth;

    public EnemyStatemachine StateMachine { get; private set; }
    public EnemyState.IdleState IdleState { get; private set; }
    public EnemyState.ChaseState ChaseState { get; private set; }
    public EnemyState.MoveState MoveState { get; private set; }
    public EnemyState.AttackState AttackState { get; private set; }
    public EnemyState.DieState DieState { get; private set; }
    private void OnEnable()
    {
        GameManager.Instance.OnPlayerReady += OnPlayerReady;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerReady -= OnPlayerReady;
    }

    private void OnPlayerReady(PlayerMovement p)
    {
        player = p;
    }
    private void Awake()
    {
 
        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;
            damageAmount = enemyData.damageAmount;
        }
        else
        {
            Debug.LogWarning($"EnemyData fehlt {gameObject.name}!");
        }

        StateMachine = new EnemyStatemachine();

        IdleState = new EnemyState.IdleState(StateMachine, this);
        ChaseState = new EnemyState.ChaseState(StateMachine, this);
        MoveState = new EnemyState.MoveState(StateMachine, this);
        AttackState = new EnemyState.AttackState(StateMachine, this);
        DieState = new EnemyState.DieState(StateMachine, this);

        StateMachine.Initialize(IdleState);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && ((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            GiveDamage(damageAmount);
        }
    }
    private void Update()
    {
        // Update den aktuellen State
        StateMachine.Update();
    }

    private void FixedUpdate()
    {
        // FixedUpdate für den aktuellen State
        StateMachine.FixedUpdate();
    }

    public void TakeDamage(int amount, DamageSource source)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void GiveDamage(int amount)
    {
        player?.TakeDamage(amount, DamageSource.Enemy);
    }

    private void Die()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.OnEnemyKilled();
        }
        StateMachine.ChangeState(DieState);
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}