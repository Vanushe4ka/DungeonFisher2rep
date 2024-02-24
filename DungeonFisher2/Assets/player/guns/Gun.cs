using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float rotationSpeed;
    public Transform transform { get; set; }
    public SpriteRenderer spriteRenderer { get; set; }
    public Animator animator { get; set; }
    public Vector2 startGunPos { get; set; }
    public GameObject bullet;
    
    public Vector2Int direction;
    private float shotClodownTimer;
    
    public float shotColdownTime = 1;
    public float barrelLong = 1;
    public int spread = 0;

    private void Start()
    {
        transform = gameObject.GetComponent<Transform>();
        startGunPos = transform.localPosition;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }
    public virtual void Update()
    {
        if (shotClodownTimer > 0) { shotClodownTimer -= Time.deltaTime; }
        if (Input.GetKeyDown(KeyCode.Mouse0) && shotClodownTimer <= 0) {Shot();shotClodownTimer = shotColdownTime; }
    }
    public virtual void Shot()
    {

    }
}

