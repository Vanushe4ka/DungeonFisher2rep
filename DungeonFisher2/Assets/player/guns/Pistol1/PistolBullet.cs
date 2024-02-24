using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolBullet : Bullet
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        rigidbody.velocity = transform.right * speed;
    }

    
}
