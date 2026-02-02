using UnityEngine;

public abstract class EnemyState
{
    protected EnemyStatemachine stateMachine;
    protected BaseEnemy enemy;

    public EnemyState(EnemyStatemachine stateMachine, BaseEnemy enemy)
    {
        this.stateMachine = stateMachine;
        this.enemy = enemy;
    }
    #region State Methods
    public virtual void Enter() {Debug.Log($"[{enemy.gameObject.name}] Entering State: {GetType().Name}");}
    public virtual void Update() {}
    public virtual void FixedUpdate() {}
    public virtual void Exit() {Debug.Log($"[{enemy.gameObject.name}] Exiting State: {GetType().Name}");}
    #endregion

    #region IdleState
    public class IdleState : EnemyState
    {

        public IdleState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

        }
    }
    #endregion

    #region MoveState

    public class MoveState : EnemyState
    {
        public MoveState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy)
        {

        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
    #endregion
    #region ChaseState
    public class ChaseState : EnemyState
    {
        public ChaseState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy)
        {

        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

        }
    }
    #endregion
    #region AttackState
    public class AttackState : EnemyState
    {
        private float attackTimer;
        private float attackDuration = 1.5f; // Wie lange angreifen

        public AttackState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy)
        {
        }

        public override void Enter()
        {
            base.Enter();
            attackTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            attackTimer += Time.deltaTime;

            // Hier würde Attack Logic kommen
            // z.B. Play Attack Animation, Deal Damage

            // Nach Attack -> zurück zu Idle
            if (attackTimer >= attackDuration)
            {
                stateMachine.ChangeState(enemy.IdleState);
            }
        }
    }
    #endregion

    #region DieState
    public class DieState : EnemyState
    {
        public DieState(EnemyStatemachine stateMachine, BaseEnemy enemy) : base(stateMachine, enemy)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // Hier würde Death Logic kommen
            // z.B. Play Death Animation, Disable Collider, etc.

            // Zerstöre nach kurzer Verzögerung
            Object.Destroy(enemy.gameObject);
        }

        public override void Update()
        {
            base.Update();

            // Im Die State passiert nichts mehr
            // Enemy wartet nur darauf zerstört zu werden
        }
    }
    #endregion
}