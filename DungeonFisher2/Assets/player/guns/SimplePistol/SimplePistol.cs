using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePistol : Gun
{
    public override void Shot()
    {
        if (player != null) { player.ShakeCamera(0.01f, 0.05f); }
        animator.SetTrigger("Shot");
        Vector3 bulletPos = transform.position;
        Quaternion bulletRot = transform.rotation;
        if (direction.y == 0) 
        {
            bulletPos += transform.right * barrelLong;
            bulletRot *= Quaternion.Euler(0, 0, Random.Range(-spread,spread));
        }
        else
        {
            bulletPos += -transform.up * barrelLong;
            bulletRot *= Quaternion.Euler(0, 0, -90 + Random.Range(-spread, spread));
        }
        Instantiate(bullet, bulletPos, bulletRot);
    }
}
