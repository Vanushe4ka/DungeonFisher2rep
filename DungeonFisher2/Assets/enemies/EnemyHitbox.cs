using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    Enemies parent;
    [Header ("0-down, 1-up,2-right,3-left")]
    public PolygonCollider2D[] HitBoxesDependingDirection;
    private void Start()
    {
        parent = gameObject.GetComponentInParent<Enemies>();
    }
    public void ChangeDirection(int dir)
    {
        if (dir >= HitBoxesDependingDirection.Length) { return; }
        for (int i = 0; i < HitBoxesDependingDirection.Length; i++) { HitBoxesDependingDirection[i].enabled = false; }
        HitBoxesDependingDirection[dir].enabled = true;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObject = collision.gameObject;
        if (otherObject.tag == "playerBullet")
        {
            parent.Damage(otherObject.GetComponent<Bullet>().damage);
            if (parent.HP <= 0) { parent.Dead(otherObject.transform.position,otherObject.GetComponent<Bullet>().speed);gameObject.layer = 10; }
            Destroy(otherObject);
        }

    }
}
