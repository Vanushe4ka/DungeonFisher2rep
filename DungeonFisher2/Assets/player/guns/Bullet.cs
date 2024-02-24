using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    public float liveTime = 10;
    public Rigidbody2D rigidbody;
    public int damage;
    public virtual void Start()
    {
        gameObject.tag = "playerBullet";
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        liveTime -= Time.deltaTime;
        if (liveTime <= 0) { Destroy(gameObject); }
    }
}
