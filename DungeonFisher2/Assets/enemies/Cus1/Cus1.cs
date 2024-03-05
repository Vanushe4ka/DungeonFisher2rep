using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cus1 : Enemies
{
    public int pushForce;
    public bool attacking = false;
    public int damageDealt = 1;
    //public float attackingTime =0.25f;
    //private float attackingTimer;
    private void OnCollisionStay2D(Collision2D collision)
    {
        GameObject otherObject = collision.gameObject;
        if (otherObject.tag == "player" && attacking)
        {
            attacking = false;
            player.Damage(damageDealt);
            Vector3 direction = (player.transform.position - transform.position).normalized;
            player.rigidbody.AddForce(direction * (pushForce / 2), ForceMode2D.Impulse);
            if (player.HP <= 0) { player.Dead(); }
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        CalculateLayer();
        if (rechargeTimer <= 0 && !isDead)
        {
            //DetermDirection();
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
                RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, Vector2.Distance(transform.position, player.transform.position), raycastLayer);
                if (hit.collider != null)// ���� ��� ���������� � ������������
                {
                    if (Path != null && Path.Count > 0 && Vector2.Distance(Path[Path.Count - 1], player.ConvertPosToMatrixCoordinate()) <= atackRadius)
                    {
                        Move();
                    }
                    else { Path = AStar.FindPath(ConvertPosToMatrixCoordinate(), player.ConvertPosToMatrixCoordinate(), dungeon, new List<int>() { 1, 3 }); }
                    Debug.DrawLine(transform.position, hit.point, Color.red); // ������ ����� �� ����� ������������
                }
                else// ���� ��� �� ���������� � ������������
                {
                    Move(player.transform.position);
                    Debug.DrawLine(transform.position, player.transform.position, Color.green); // ������ ����� �� ��������� �� �������� �����
                }

                
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
        DetermDirection(player.transform.position);
        animator.SetTrigger("atack");
        attacking = true;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        rigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
        rechargeTimer = rechargeTime;
    }
    public void EndAtacking()
    {
        attacking = false;
    }
}
