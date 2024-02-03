using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rigidbody;
    public float moveSpeed;
    Vector2 prevMoveDir = Vector2.zero;
    public Transform cameraTransform;
    public float cameraSpeed;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        cameraTransform.position = Vector3.Lerp(cameraTransform.position , new Vector3(gameObject.transform.position.x, gameObject.transform.position.y,-10), cameraSpeed);
        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        rigidbody.MovePosition(rigidbody.position + moveDirection * moveSpeed);
        
        
    }
}
