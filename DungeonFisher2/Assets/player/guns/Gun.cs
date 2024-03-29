using System.Collections;
using System.Collections.Generic;
using Water2D;
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
    protected float shotClodownTimer;
    
    public float shotColdownTime = 1;
    public float barrelLong = 1;
    public int spread = 0;
    public Player player;
    public Transform reflectionPivot;
    public Reflector reflector;
    public SpriteRenderer reflectionRenderer;

    private void Start()
    {
        transform = gameObject.GetComponent<Transform>();
        startGunPos = transform.localPosition;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }
    public virtual void Update()
    {
        reflectionPivot.localPosition = new Vector3(transform.localPosition.x, -0.9f - 0.25f - transform.localPosition.y, transform.localPosition.z);
        reflectionPivot.rotation = Quaternion.Inverse(transform.rotation);
        if (direction.y != 0) { reflector.raymarched = true; }
        else { reflector.raymarched = false; }
        if (spriteRenderer.flipY) { reflectionPivot.transform.localScale = new Vector3(-1, 1, 1); }
        else { reflectionPivot.transform.localScale = new Vector3(-1, -1, 1); }
        

        if (shotClodownTimer > 0) { shotClodownTimer -= Time.deltaTime; }
    }
    public virtual void Shot()
    {

    }
}

