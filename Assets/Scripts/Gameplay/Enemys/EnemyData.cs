using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;

    [Header("Flags")]
    public bool canIdle;
    public bool canWalk;
    public bool canFly;
    public bool canAttack;
    public bool canDie;

    [Header("Stats")]
    public int maxHealth = 4;
    public int damageAmount = 1;
    public float moveSpeed = 3f;

    [Header("Combat")]
    public float patrolChangeTime = 1.5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Attacks")]
    //public List<EnemyAttackData> attacks;

    [Header("Effects")]
    public GameObject deathEffect;

    [Header("Visual")]
    public RuntimeAnimatorController animator;
}