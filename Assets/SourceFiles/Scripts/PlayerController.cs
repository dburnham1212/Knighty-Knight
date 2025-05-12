using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharacterScript
{
    // private members
    // InputActions
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    // Layers
    // attack stuff
    Collider2D[] enemies;
    List<Collider2D> hitEnemies;
    Vector2 attackPosition;
    float attackRadius;
    int attackIterator;

    // properties
    public Inventory Inventory { get; private set; }

    // Unity Messages
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Initialize();
        // initialize InputActions
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        attackAction = InputSystem.actions.FindAction("Attack");

        // initialize properties
        Inventory = new Inventory();
    }

    // FixedUpdate is called once per fixed-frame
    private void FixedUpdate()
    {
        UpdateStart();

        MoveValue = moveAction.ReadValue<Vector2>();

        CheckGround();
        CheckPlatform();

        if (!Animator.GetBool("isAttacking"))
        {
            FlipSprite();

            if (jumpAction.WasPressedThisFrame() && !IsJumping && CanJump())
            {
                if (IsOnPlatform)
                    LeavePlatform();

                if (!IsOnPlatform || MoveValue.y >= 0)
                    Jump();
            }

            if (attackAction.WasPressedThisFrame())
                Animator.SetBool("isAttacking", true);
        }

        HorizontalMovement();

        PreviousPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnter();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionStay();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionExit(collision, jumpAction.WasPressedThisFrame());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            int index = (int)item.itemName;
            print(index);

            switch (item.type)
            {
                case Inventory.Type.Consumable:
                    index -= Inventory.KEY_ITEMS;
                    print(index);

                    if (index >= 0 && index < Inventory.CONSUMABLES)
                        Inventory.Consumables[index].Add(item.count);
                    else
                        print("type and name mismatch");
                    break;
                case Inventory.Type.Upgrade:
                    index -= Inventory.KEY_ITEMS;
                    print(index);

                    if (index >= 0 && index < Inventory.CONSUMABLES)
                        Inventory.Consumables[index].Upgrade();
                    else
                        print("type and name mismatch");
                    break;
                default:
                    if (index >= 0 && index < Inventory.KEY_ITEMS)
                        Inventory.KeyItems[index].Upgrade();
                    else
                        print("type and name mismatch");
                    break;
            }

            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        DrawGizmos();

        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }

    // animation events
    public void AttackStart()
    {
        attackIterator = 0;
        hitEnemies = new List<Collider2D>();
    }

    public void Attack()
    {
        attackPosition.x = !SpriteRenderer.flipX ?
            transform.position.x + transform.localScale.x / 2 :
            transform.position.x - transform.localScale.x / 2;

        switch (attackIterator)
        {
            case 1:
                attackPosition.y = transform.position.y + transform.localScale.y / 2 * 4 / 6;
                break;
            case 2:
                attackPosition.y = transform.position.y - transform.localScale.y / 2 * 2 / 6;
                break;
            case 3:
                attackPosition.y = transform.position.y - transform.localScale.y / 2 * 4 / 6;
                break;
            default:
                attackRadius = 0.25f;
                attackPosition.y = transform.position.y + transform.localScale.y / 2 * 4 / 6;
                break;
        }

        enemies = Physics2D.OverlapCircleAll(attackPosition, attackRadius, EnemyMask);
        foreach (Collider2D enemy in enemies)
        {
            EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();

            if (enemyScript.CanBeHit)
            {
                enemyScript.Health -= 3;

                if (enemyScript.Health > 0)
                {
                    hitEnemies.Add(enemy);
                    enemyScript.CanBeHit = false;
                }
                else
                    Destroy(enemy.gameObject);
            }
        }

        attackIterator++;
    }

    public void AttackEnd()
    {
        for (int index = 0; index < hitEnemies.Count; index++)
        {
            EnemyScript enemyScript = hitEnemies[index].GetComponent<EnemyScript>();
            enemyScript.CanBeHit = true;
            hitEnemies.Remove(hitEnemies[index]);
        }

        attackRadius = 0;
        Animator.SetBool("isAttacking", false);
    }
}
