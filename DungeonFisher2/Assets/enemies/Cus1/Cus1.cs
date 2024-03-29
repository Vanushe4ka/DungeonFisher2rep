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
                Vector2 lineVector = (player.transform.position - transform.position).normalized;

                // ������� ������, ���������������� ����� ����� �������
                Vector2 perpendicularVector = new Vector2(-lineVector.y, lineVector.x);

                // ��������� ���������� ��������������� �����
                Vector2 extremeColliderPoint1 = (Vector2)transform.position + perpendicularVector * collider.radius * transform.localScale;
                Vector2 extremeColliderPoint2 = (Vector2)transform.position - perpendicularVector * collider.radius * transform.localScale;
                Debug.DrawLine(extremeColliderPoint2, extremeColliderPoint1, Color.yellow);

                RaycastHit2D hit1 = Physics2D.Raycast(extremeColliderPoint1, player.transform.position - (Vector3)extremeColliderPoint1, Vector2.Distance(extremeColliderPoint1, player.transform.position), raycastLayer);
                RaycastHit2D hit2 = Physics2D.Raycast(extremeColliderPoint2, player.transform.position -(Vector3)extremeColliderPoint2, Vector2.Distance(extremeColliderPoint2, player.transform.position), raycastLayer);
                if (hit1.collider != null || hit2.collider != null)// ���� ���� ���� ��� ���������� � ������������
                {
                    if (Path != null && Path.Count > 0 && Vector2.Distance(Path[Path.Count - 1], player.ConvertPosToMatrixCoordinate()) <= atackRadius)
                    {
                        Move();
                    }
                    else { Path = AStar.FindPath(ConvertPosToMatrixCoordinate(), player.ConvertPosToMatrixCoordinate(), dungeon, new List<int>() { 1, 3 }); }
                    Debug.DrawLine(player.transform.position, hit1.point, Color.red);
                    Debug.DrawLine(player.transform.position, hit2.point, Color.red);// ������ ����� �� ����� ������������
                }
                else// ���� ���� �� ����������� � ������������
                {
                    Move(player.transform.position);
                    Debug.DrawLine(extremeColliderPoint1, player.transform.position, Color.green);
                    Debug.DrawLine(extremeColliderPoint2, player.transform.position, Color.green);// ������ ����� �� ��������� �� �������� �����
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
