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
    
    private void Start()
    {
        transform = gameObject.GetComponent<Transform>();
        startGunPos = transform.localPosition;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }
}

