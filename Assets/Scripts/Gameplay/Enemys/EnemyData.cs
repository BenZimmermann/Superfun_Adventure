using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

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
    public float moveSpeed = 3f;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float chaseRange = 12f;
    public float patrolChangeTime = 1.5f;

    [Header("Attacks")]
    public List<AttackData> attacks;

    [Header("Effects")]
    public GameObject deathEffect;

    [Header("Visual")]
    public RuntimeAnimatorController animator;
}