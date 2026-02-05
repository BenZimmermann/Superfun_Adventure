using UnityEngine;

/// <summary>
/// Enemy State System - Cuphead/Kirby inspired behavior
/// </summary>
public abstract class EnemyState
{
    protected EnemyStatemachine stateMachine;
    protected BaseEnemy enemy;

    protected EnemyState(EnemyStatemachine stateMachine, BaseEnemy enemy)
    {
        this.stateMachine = stateMachine;
        this.enemy = enemy;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }

    protected bool TryChangeState(EnemyState newState)
    {
        if (newState != null && stateMachine != null)
        {
            stateMachine.ChangeState(newState);
            return true;
        }
        return false;
    }

    #region IdleState

    public class IdleState : EnemyState
    {
        private float timer;

        public IdleState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            timer = 0f;
            enemy.StopMovement(immediate: false); // Smooth stop
            enemy.SetAnimationBool("IsMoving", false);
            enemy.SetAnimationBool("IsChasing", false);
        }

        public override void Update()
        {
            if (enemy?.EnemyData == null) return;

            // Check player detection
            if (enemy.EnemyData.canAttack && enemy.IsPlayerInDetectionRange())
            {
                TryChangeState(enemy.ChaseState);
                return;
            }

            timer += Time.deltaTime;

            float waitTime = enemy.WaitTimeAtWaypoint > 0
                ? enemy.WaitTimeAtWaypoint
                : enemy.EnemyData.patrolChangeTime;

            if (timer >= waitTime && enemy.PatrolWaypoints != null && enemy.PatrolWaypoints.Length > 0)
            {
                TryChangeState(enemy.MoveState);
            }
        }
    }

    #endregion

    #region MoveState

    public class MoveState : EnemyState
    {
        private const float WAYPOINT_REACH_THRESHOLD = 0.5f;
        private const float EDGE_CHECK_DISTANCE = 1.2f;
        private const float EDGE_CHECK_HEIGHT = 0.5f;

        private float bobTimer = 0f; // For floating enemies

        public MoveState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            if (enemy.PatrolWaypoints == null || enemy.PatrolWaypoints.Length == 0)
            {
                TryChangeState(enemy.IdleState);
                return;
            }

            bobTimer = 0f;
            enemy.SetAnimationBool("IsMoving", true);
            enemy.SetAnimationBool("IsChasing", false);
        }

        public override void Update()
        {
            if (enemy?.EnemyData == null) return;

            // Check player detection
            if (enemy.EnemyData.canAttack && enemy.IsPlayerInDetectionRange())
            {
                TryChangeState(enemy.ChaseState);
                return;
            }

            Transform targetWaypoint = enemy.GetCurrentWaypoint();
            if (targetWaypoint != null)
            {
                float distance = Vector2.Distance(enemy.transform.position, targetWaypoint.position);

                if (distance < WAYPOINT_REACH_THRESHOLD)
                {
                    enemy.NextWaypoint();
                    TryChangeState(enemy.IdleState);
                }
            }
            else
            {
                TryChangeState(enemy.IdleState);
            }
        }

        public override void FixedUpdate()
        {
            if (enemy?.Rb == null || enemy.EnemyData == null) return;

            Transform targetWaypoint = enemy.GetCurrentWaypoint();
            if (targetWaypoint == null) return;

            Vector2 targetPos = targetWaypoint.position;

            // Add bobbing motion for flying enemies (Cuphead-style)
            if (enemy.EnemyData.canFly)
            {
                bobTimer += Time.fixedDeltaTime * 3f;
                float bobOffset = Mathf.Sin(bobTimer) * 0.3f;
                targetPos.y += bobOffset;
            }

            // Check for edge if walking
            if (enemy.EnemyData.canWalk)
            {
                Vector2 currentPos = enemy.transform.position;
                float directionX = Mathf.Sign(targetPos.x - currentPos.x);

                if (IsEdgeAhead(directionX))
                {
                    enemy.StopMovement(immediate: true);
                    enemy.NextWaypoint();
                    TryChangeState(enemy.IdleState);
                    return;
                }
            }

            // Smooth movement
            enemy.MoveTowards(targetPos, enemy.EnemyData.moveSpeed, smoothing: true);
        }

        public override void Exit()
        {
            enemy.SetAnimationBool("IsMoving", false);
        }

        private bool IsEdgeAhead(float direction)
        {
            Vector2 currentPos = enemy.transform.position;
            Vector2 checkPosition = currentPos + new Vector2(direction * EDGE_CHECK_DISTANCE, -EDGE_CHECK_HEIGHT);

            RaycastHit2D hit = Physics2D.Raycast(
                checkPosition,
                Vector2.down,
                EDGE_CHECK_HEIGHT * 2,
                LayerMask.GetMask("Ground")
            );

            Debug.DrawRay(checkPosition, Vector2.down * EDGE_CHECK_HEIGHT * 2,
                hit.collider != null ? Color.green : Color.red);

            return hit.collider == null;
        }
    }

    #endregion

    #region ChaseState

    public class ChaseState : EnemyState
    {
        private const float HOVER_HEIGHT = 2.5f;
        private const float EDGE_CHECK_DISTANCE = 1.2f;
        private const float EDGE_CHECK_HEIGHT = 0.5f;
        private const float ANTICIPATION_DISTANCE = 1f; // Kirby-style positioning

        private float bobTimer = 0f;
        private AttackData currentAttack = null;

        public ChaseState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            if (enemy.PlayerTransform == null)
            {
                TryChangeState(enemy.IdleState);
                return;
            }

            bobTimer = 0f;
            enemy.SetAnimationBool("IsChasing", true);
            enemy.SetAnimationBool("IsMoving", true);
        }

        public override void Update()
        {
            if (enemy?.EnemyData == null) return;

            // Check if player is lost
            if (enemy.PlayerTransform == null || !enemy.IsPlayerInChaseRange())
            {
                TryChangeState(enemy.IdleState);
                return;
            }

            // Check for available attack
            currentAttack = enemy.GetAvailableAttack();
            if (currentAttack != null && enemy.IsPlayerInAttackRange(currentAttack.range))
            {
                TryChangeState(enemy.AttackState);
                return;
            }
        }

        public override void FixedUpdate()
        {
            if (enemy?.Rb == null || enemy.EnemyData == null || enemy.PlayerTransform == null)
                return;

            if (enemy.EnemyData.canFly)
            {
                ChaseAsFlying();
            }
            else if (enemy.EnemyData.canWalk)
            {
                ChaseAsWalking();
            }
        }

        public override void Exit()
        {
            enemy.SetAnimationBool("IsChasing", false);
        }

        private void ChaseAsFlying()
        {
            Vector2 playerPos = enemy.PlayerTransform.position;

            // Add bobbing motion (Cuphead-style)
            bobTimer += Time.fixedDeltaTime * 4f;
            float bobOffset = Mathf.Sin(bobTimer) * 0.4f;

            Vector2 targetPos = playerPos + Vector2.up * (HOVER_HEIGHT + bobOffset);

            // Smooth chase
            enemy.MoveTowards(targetPos, enemy.EnemyData.moveSpeed * 1.2f, smoothing: true);
        }

        private void ChaseAsWalking()
        {
            Vector2 currentPos = enemy.transform.position;
            Vector2 playerPos = enemy.PlayerTransform.position;

            // Anticipate player position (Kirby-style)
            Vector2 targetPos = playerPos;
            float playerDirection = Mathf.Sign(playerPos.x - currentPos.x);

            // Stop at anticipation distance
            float distanceToPlayer = Mathf.Abs(playerPos.x - currentPos.x);
            if (distanceToPlayer > ANTICIPATION_DISTANCE)
            {
                // Check for edge
                if (IsEdgeAhead(playerDirection))
                {
                    enemy.StopMovement(immediate: false);
                    TryChangeState(enemy.IdleState);
                    return;
                }

                // Move towards player
                enemy.MoveTowards(playerPos, enemy.EnemyData.moveSpeed * 1.1f, smoothing: true);
            }
            else
            {
                // At anticipation distance - slow down
                enemy.StopMovement(immediate: false);
            }
        }

        private bool IsEdgeAhead(float direction)
        {
            Vector2 currentPos = enemy.transform.position;
            Vector2 checkPosition = currentPos + new Vector2(direction * EDGE_CHECK_DISTANCE, -EDGE_CHECK_HEIGHT);

            RaycastHit2D hit = Physics2D.Raycast(
                checkPosition,
                Vector2.down,
                EDGE_CHECK_HEIGHT * 2,
                LayerMask.GetMask("Ground")
            );

            return hit.collider == null;
        }
    }

    #endregion

    #region AttackState

    public class AttackState : EnemyState
    {
        private AttackData currentAttack;
        private float postAttackWaitTimer;
        private const float POST_ATTACK_WAIT = 0.3f; // Kurze Pause nach Attack

        public AttackState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            postAttackWaitTimer = 0f;
            currentAttack = enemy.GetAvailableAttack();

            if (currentAttack == null)
            {
                TryChangeState(enemy.ChaseState);
                return;
            }

            // Stop movement if required
            if (currentAttack.lockMovementDuringAttack)
            {
                enemy.StopMovement(immediate: true);
            }

            // Face player
            Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
            enemy.SetFacingDirection(directionToPlayer.x);

            enemy.SetAnimationBool("IsAttacking", true);

            // Perform attack
            enemy.PerformAttack(currentAttack);
        }

        public override void Update()
        {
            if (enemy?.EnemyData == null) return;

            // Wait for attack to finish
            if (enemy.IsAttacking) return;

            // Short wait after attack ends
            postAttackWaitTimer += Time.deltaTime;

            if (postAttackWaitTimer < POST_ATTACK_WAIT)
                return;

            // Check if player is out of chase range
            if (!enemy.IsPlayerInChaseRange())
            {
                TryChangeState(enemy.IdleState);
                return;
            }

            // Check if still in attack range
            AttackData nextAttack = enemy.GetAvailableAttack();
            if (nextAttack != null && enemy.IsPlayerInAttackRange(nextAttack.range))
            {
                // Check cooldown
                if (currentAttack != null && postAttackWaitTimer >= currentAttack.cooldown)
                {
                    // Ready for next attack - restart state
                    TryChangeState(enemy.AttackState);
                }
                // Still in cooldown, stay in attack state and wait
            }
            else
            {
                // Player moved out of attack range but still in chase range
                // Return to chase
                TryChangeState(enemy.ChaseState);
            }
        }

        public override void Exit()
        {
            enemy.SetAnimationBool("IsAttacking", false);
        }
    }

    #endregion

    #region DieState

    public class DieState : EnemyState
    {
        private const float DEATH_DELAY = 1.5f;
        private float deathTimer;

        public DieState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            deathTimer = 0f;

            if (enemy?.Rb != null)
            {
                enemy.Rb.linearVelocity = Vector2.zero;
                enemy.Rb.gravityScale = 0f;
                enemy.Rb.bodyType = RigidbodyType2D.Static;
            }

            // Disable colliders
            var colliders = enemy.GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Trigger death animation
            enemy.TriggerAnimation("Die");

            // Spawn death effect
            if (enemy.EnemyData?.deathEffect != null)
            {
                Object.Instantiate(
                    enemy.EnemyData.deathEffect,
                    enemy.transform.position,
                    Quaternion.identity
                );
            }
        }

        public override void Update()
        {
            if (enemy == null) return;

            deathTimer += Time.deltaTime;

            // Fade out effect (optional)
            var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                float alpha = 1f - (deathTimer / DEATH_DELAY);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }

            if (deathTimer >= DEATH_DELAY)
            {
                Debug.Log($"[{enemy.name}] Destroying enemy after death.");
                Object.Destroy(enemy.gameObject);
            }
        }
    }

    #endregion
}