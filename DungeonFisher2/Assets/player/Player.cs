using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rigidbody;
    SpriteRenderer spriteRenderer;
    Animator animator;
    public float moveSpeed;
    public float animMoveSpeed;
    Vector2Int rotateDirection = new Vector2Int(0, -1);
    public Transform cameraTransform;
    public float cameraSpeed;
    public Gun[] AllGuns;
    public Gun ActiveGun;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }
    public void CalculateLayer()
    {
        spriteRenderer.sortingOrder = 400 - Mathf.RoundToInt(transform.position.y*4);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        CalculateLayer();
        cameraTransform.position = Vector3.Lerp(cameraTransform.position , new Vector3(gameObject.transform.position.x, gameObject.transform.position.y,-10), cameraSpeed);
        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        if (moveDirection.x != 0 || moveDirection.y != 0)
        {
            rigidbody.MovePosition(rigidbody.position + moveDirection * moveSpeed);
            animator.SetBool("run", true);
            if ((moveDirection.x != 0 && rotateDirection.x != 0 && moveDirection.x != rotateDirection.x) || (moveDirection.y != 0 && rotateDirection.y != 0 && moveDirection.y != rotateDirection.y))
            {
                animator.SetFloat("runSpeed", -animMoveSpeed);
            }
            else
            {
                animator.SetFloat("runSpeed", animMoveSpeed);
            }
            
        }
        else
        {
            animator.SetBool("run", false);
        }
        
        
        //позиция курсора мыши
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        spriteRenderer.flipX = (mousePos.x < transform.position.x);

        //поворот текущей пушки за курсором
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //Debug.Log(Mathf.Cos(angle * Mathf.PI / 180));
        if (angle <= 135 && angle > 45)
        {
            ActiveGun.spriteRenderer.flipY = false;
            ActiveGun.spriteRenderer.flipX = false;
            if (!ActiveGun.animator.GetBool("isDownDirection"))
            {
                //получение инфы о текущей анимации
                AnimatorClipInfo[] currentClipInfo = ActiveGun.animator.GetCurrentAnimatorClipInfo(0);
                if (currentClipInfo[0].clip.name == "SideShotAnim")//если это анимация выстрела в бок
                {
                    float normalizedTime = ActiveGun.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    ActiveGun.animator.Play("DownShotAnim", 0, normalizedTime);
                }
                ActiveGun.animator.SetBool("isDownDirection", true);
            }
            ActiveGun.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            Quaternion rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
            ActiveGun.transform.rotation = Quaternion.Slerp(ActiveGun.transform.rotation, rotation, ActiveGun.rotationSpeed * Time.deltaTime);

            animator.SetInteger("direction", 1);
            rotateDirection = new Vector2Int(0,1);
        }
        else if (angle <= 45 && angle > -45)
        {
            ActiveGun.spriteRenderer.flipY = false;
            ActiveGun.spriteRenderer.flipX = false;
            if (ActiveGun.animator.GetBool("isDownDirection"))
            {
                //получение инфы о текущей анимации
                AnimatorClipInfo[] currentClipInfo = ActiveGun.animator.GetCurrentAnimatorClipInfo(0);
                if (currentClipInfo[0].clip.name == "DownShotAnim")//если это анимация выстрела в низ
                {
                    float normalizedTime = ActiveGun.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    ActiveGun.animator.Play("SideShotAnim", 0, normalizedTime);
                }
                ActiveGun.animator.SetBool("isDownDirection", false);
            }
            
            ActiveGun.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            ActiveGun.transform.rotation = Quaternion.Slerp(ActiveGun.transform.rotation, rotation, ActiveGun.rotationSpeed * Time.deltaTime);

            animator.SetInteger("direction", 2);
            rotateDirection = new Vector2Int(1, 0);
        }
        else if (angle <= -45 && angle>-135)
        {
            ActiveGun.spriteRenderer.flipY = false;
            ActiveGun.spriteRenderer.flipX = false;
            if (!ActiveGun.animator.GetBool("isDownDirection"))
            {
                //получение инфы о текущей анимации
                AnimatorClipInfo[] currentClipInfo = ActiveGun.animator.GetCurrentAnimatorClipInfo(0);
                if (currentClipInfo[0].clip.name == "SideShotAnim")//если это анимация выстрела в бок
                {
                    float normalizedTime = ActiveGun.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    ActiveGun.animator.Play("DownShotAnim", 0, normalizedTime);
                }
                ActiveGun.animator.SetBool("isDownDirection", true);
            }
            ActiveGun.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            Quaternion rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
            ActiveGun.transform.rotation = Quaternion.Slerp(ActiveGun.transform.rotation, rotation, ActiveGun.rotationSpeed * Time.deltaTime);

            animator.SetInteger("direction", 0);
            rotateDirection = new Vector2Int(0, -1);
        }
        else if ((angle <= -135 && angle > -180) || (angle <= 180 && angle > 135))
        {
            ActiveGun.spriteRenderer.flipY = true;
            ActiveGun.spriteRenderer.flipX = false;
            if (ActiveGun.animator.GetBool("isDownDirection"))
            {
                //получение инфы о текущей анимации
                AnimatorClipInfo[] currentClipInfo = ActiveGun.animator.GetCurrentAnimatorClipInfo(0);
                if (currentClipInfo[0].clip.name == "DownShotAnim")//если это анимация выстрела в низ
                {
                    float normalizedTime = ActiveGun.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    ActiveGun.animator.Play("SideShotAnim", 0, normalizedTime);
                }
                ActiveGun.animator.SetBool("isDownDirection", false);
            }
            ActiveGun.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            ActiveGun.transform.rotation = Quaternion.Slerp(ActiveGun.transform.rotation, rotation, ActiveGun.rotationSpeed * Time.deltaTime);

            animator.SetInteger("direction", 2);
            rotateDirection = new Vector2Int(-1, 0);
        }
        ActiveGun.direction = rotateDirection;
        ActiveGun.transform.localPosition = new Vector3(ActiveGun.startGunPos.x * Mathf.Cos(angle * Mathf.PI / 180), ActiveGun.startGunPos.y * -Mathf.Sin(angle * Mathf.PI / 180), 0);

    }
}
