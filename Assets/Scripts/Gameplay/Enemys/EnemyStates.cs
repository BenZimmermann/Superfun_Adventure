using UnityEngine;

/// <summary>
/// Enemy State System - Cuphead/Kirby inspired behavior
/// Unity 6 compatible mit korrekter Animations-Priorität
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
            enemy.StopMovement(immediate: false);

            // Setze ALLE Animations-Bools auf false, dann nur Idle auf true
            enemy.SetAnimationBool("IsMoving", false);
            enemy.SetAnimationBool("IsAttacking", false);
            enemy.SetAnimationBool("IsIdleing", true);
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

        private float bobTimer = 0f;

        public MoveState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            if (enemy.PatrolWaypoints == null || enemy.PatrolWaypoints.Length == 0)
            {
                TryChangeState(enemy.IdleState);
                return;
            }

            bobTimer = 0f;

            // Setze alle anderen Bools auf false
            enemy.SetAnimationBool("IsIdleing", false);
            enemy.SetAnimationBool("IsAttacking", false);
            enemy.SetAnimationBool("IsMoving", true);
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

            // Add bobbing motion for flying enemies
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
        private const float ANTICIPATION_DISTANCE = 1f;

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

            // Setze alle anderen Bools auf false
            enemy.SetAnimationBool("IsIdleing", false);
            enemy.SetAnimationBool("IsAttacking", false);
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
            enemy.SetAnimationBool("IsMoving", false);
        }

        private void ChaseAsFlying()
        {
            Vector2 playerPos = enemy.PlayerTransform.position;

            bobTimer += Time.fixedDeltaTime * 4f;
            float bobOffset = Mathf.Sin(bobTimer) * 0.4f;

            Vector2 targetPos = playerPos + Vector2.up * (HOVER_HEIGHT + bobOffset);

            enemy.MoveTowards(targetPos, enemy.EnemyData.moveSpeed * 1.2f, smoothing: true);
        }

        private void ChaseAsWalking()
        {
            Vector2 currentPos = enemy.transform.position;
            Vector2 playerPos = enemy.PlayerTransform.position;

            Vector2 targetPos = playerPos;
            float playerDirection = Mathf.Sign(playerPos.x - currentPos.x);

            float distanceToPlayer = Mathf.Abs(playerPos.x - currentPos.x);
            if (distanceToPlayer > ANTICIPATION_DISTANCE)
            {
                if (IsEdgeAhead(playerDirection))
                {
                    enemy.StopMovement(immediate: false);
                    TryChangeState(enemy.IdleState);
                    return;
                }

                enemy.MoveTowards(playerPos, enemy.EnemyData.moveSpeed * 1.1f, smoothing: true);
            }
            else
            {
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
        private enum AttackPhase
        {
            WindUp,      // Animation wird abgespielt
            Execute,     // Angriff wird ausgeführt
            Recovery,    // Nach dem Angriff
            Cooldown     // Warten vor nächstem Angriff
        }

        private AttackData currentAttack;
        private float phaseTimer;
        private AttackPhase currentPhase;
        private bool attackExecuted;

        public AttackState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy) { }

        public override void Enter()
        {
            currentAttack = enemy.GetAvailableAttack();

            if (currentAttack == null)
            {
                TryChangeState(enemy.ChaseState);
                return;
            }

            // KRITISCH: Setze ALLE anderen Animations-Bools auf FALSE
            enemy.SetAnimationBool("IsMoving", false);
            enemy.SetAnimationBool("IsIdleing", false);

            StartWindUpPhase();
        }

        public override void Update()
        {
            if (enemy?.EnemyData == null) return;

            // Check if player left chase range entirely
            if (!enemy.IsPlayerInChaseRange())
            {
                TryChangeState(enemy.IdleState);
                return;
            }

            phaseTimer += Time.deltaTime;

            switch (currentPhase)
            {
                case AttackPhase.WindUp:
                    UpdateWindUpPhase();
                    break;

                case AttackPhase.Execute:
                    UpdateExecutePhase();
                    break;

                case AttackPhase.Recovery:
                    UpdateRecoveryPhase();
                    break;

                case AttackPhase.Cooldown:
                    UpdateCooldownPhase();
                    break;
            }
        }

        private void StartWindUpPhase()
        {
            currentPhase = AttackPhase.WindUp;
            phaseTimer = 0f;
            attackExecuted = false;

            // Stop movement
            if (currentAttack.lockMovementDuringAttack)
            {
                enemy.StopMovement(immediate: true);
            }

            // Face player
            Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
            enemy.SetFacingDirection(directionToPlayer.x);

            // KRITISCH: Setze NUR IsAttacking auf true
            enemy.SetAnimationBool("IsAttacking", true);

            Debug.Log($"[{enemy.name}] Starting attack wind-up animation. Duration: {currentAttack.windupTime}s");
        }

        private void UpdateWindUpPhase()
        {
            // Warte auf die Wind-Up Zeit (sollte mit deiner Attack-Animation-Länge übereinstimmen)
            float windUpDuration = currentAttack.windupTime;

            if (phaseTimer >= windUpDuration)
            {
                Debug.Log($"[{enemy.name}] Wind-up complete, executing attack!");
                currentPhase = AttackPhase.Execute;
                phaseTimer = 0f;
                ExecuteAttack();
            }
        }

        private void ExecuteAttack()
        {
            if (attackExecuted) return;

            // JETZT wird der eigentliche Angriff ausgeführt (Projektil spawnen, etc.)
            enemy.PerformAttack(currentAttack);
            attackExecuted = true;

            Debug.Log($"[{enemy.name}] Attack executed!");
        }

        private void UpdateExecutePhase()
        {
            // Kurze Pause während der Angriff aktiv ist
            float executeDuration = currentAttack.attackDuration;

            if (phaseTimer >= executeDuration)
            {
                currentPhase = AttackPhase.Recovery;
                phaseTimer = 0f;

                Debug.Log($"[{enemy.name}] Attack execute phase done, entering recovery");
            }
        }

        private void UpdateRecoveryPhase()
        {
            // Recovery Phase - Animation läuft zurück zu Idle
            float recoveryDuration = 0.2f;

            if (phaseTimer >= recoveryDuration)
            {
                currentPhase = AttackPhase.Cooldown;
                phaseTimer = 0f;

                // Beende Attack Animation
                enemy.SetAnimationBool("IsAttacking", false);

                Debug.Log($"[{enemy.name}] Recovery done, entering cooldown");
            }
        }

        private void UpdateCooldownPhase()
        {
            // Check if player is still in attack range
            AttackData nextAttack = enemy.GetAvailableAttack();

            if (nextAttack == null || !enemy.IsPlayerInAttackRange(nextAttack.range))
            {
                // Player moved out - return to chase
                Debug.Log($"[{enemy.name}] Player out of range, returning to chase");
                TryChangeState(enemy.ChaseState);
                return;
            }

            // Cooldown finished - perform next attack
            if (phaseTimer >= currentAttack.cooldown)
            {
                Debug.Log($"[{enemy.name}] Cooldown complete, starting new attack");
                currentAttack = nextAttack;

                // Setze Attack Bool wieder auf true für neuen Angriff
                enemy.SetAnimationBool("IsAttacking", true);
                StartWindUpPhase();
            }
        }

        public override void Exit()
        {
            // Stelle sicher, dass Attack Animation beendet wird
            enemy.SetAnimationBool("IsAttacking", false);

            Debug.Log($"[{enemy.name}] Exiting attack state");
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

            // Stoppe alle Animationen
            enemy.SetAnimationBool("IsMoving", false);
            enemy.SetAnimationBool("IsAttacking", false);
            enemy.SetAnimationBool("IsIdleing", false);

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
            //enemy.TriggerAnimation("Die");

            // Spawn death effect
            if (enemy.EnemyData?.deathEffect != null)
            {
                Object.Instantiate(
                    enemy.EnemyData.deathEffect,
                    enemy.transform.position,
                    Quaternion.identity
                );
            }
            Object.Destroy(enemy.gameObject);
        }

        public override void Update()
        {
            if (enemy == null) return;

            deathTimer += Time.deltaTime;

            // Fade out effect
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
                Object.Destroy(enemy.gameObject);
            }
        }
    }

    #endregion
}