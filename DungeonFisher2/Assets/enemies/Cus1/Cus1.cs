using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cus1 : Enemies
{
    public int pushForce; 
    void FixedUpdate()
    {
        CalculateLayer();
        if (rechargeTimer <= 0)
        {
            DetermDirection();
            animator.SetBool("run", true);
            if (direction.x == -1) 
            { 
                spriteRenderer.flipX = true;
                hitBox.ChangeDirection(3);
            }
            else { spriteRenderer.flipX = false; hitBox.ChangeDirection(2); }
            if (direction.y == 0) { animator.SetInteger("direction", 2); }
            if (direction.y == 1) { animator.SetInteger("direction", 1); hitBox.ChangeDirection(1); }
            if (direction.y == -1) { animator.SetInteger("direction", 0); hitBox.ChangeDirection(0); }

            if (Vector2.Distance(player.transform.position, transform.position) > atackRadius)
            {
                if (Path.Count > 0 && Path[Path.Count-1] == new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y)))
                {
                    Move();
                }
                else { AStar(); }
            }
            else
            {
                Atack();
            }
        }
        else 
        {
            rechargeTimer -= Time.deltaTime;
            animator.SetBool("run", false);
        }
    }
    public override void Atack()
    {
        animator.SetTrigger("atack");
        Vector3 direction = (player.transform.position - transform.position).normalized;
        rigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
        rechargeTimer = rechargeTime;
    }
}
