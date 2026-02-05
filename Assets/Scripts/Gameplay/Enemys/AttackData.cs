using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Enemy/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Base")]
    public string attackName;
    public EnemyAttackType attackType;

    [Header("Stats")]
    public int damage = 1;
    public float range = 1.5f;
    public float cooldown = 1f;
    public float windupTime = 0.2f;
    public float attackDuration = 0.3f;

    [Header("Spawn Points")]
    public Transform[] attackPoints;

    [Header("Visuals")]
    public GameObject attackEffectPrefab;

    [Header("Flags")]
    public bool requiresLineOfSight = true;
    public bool lockMovementDuringAttack = true;
}
public enum EnemyAttackType
{
    Slap,
    Poke,
    Laser,
    Projectile,
    DualLaser,
}
