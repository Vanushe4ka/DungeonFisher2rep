using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rigidbody;
    SpriteRenderer spriteRenderer;
    Animator animator;
    public float moveSpeed;
    public float animMoveSpeed;
    Vector2Int rotateDirection = new Vector2Int(0, -1);
    public Transform cameraTransform;
    public float cameraSpeed;
    public Gun[] AllGuns;
    public Gun ActiveGun;

    public int HP;
    public int maxHP;
    public GameObject[] HPImages;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        DrawHP();
    }
    public void CalculateLayer()
    {
        spriteRenderer.sortingOrder = 400 - Mathf.RoundToInt(transform.position.y*4);
    }
    public void Damage(int hp)
    {
        HP -= hp;
        HP = Mathf.Max(HP, 0);
        StartCoroutine(Blink());
        DrawHP();
    }
    IEnumerator Blink()
    {
        float elapsedTime = 0f;
        float blinkDuration = 0.15f;
        while (elapsedTime < blinkDuration)
        {
            float t = Mathf.Cos(elapsedTime / blinkDuration * 2 * Mathf.PI); // »нтерпол€ци€ синуса от 0 до PI
            float alpha = t / 8 + 0.75f; // ќграничение альфа не менее minAlpha

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            elapsedTime += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }

        // ”становить альфа обратно на 1 в конце анимации
        Color endColor = spriteRenderer.color;
        endColor.a = 1f;
        spriteRenderer.color = endColor;
    }
    public void AddMaxHP(int hp)
    {
        maxHP += hp;
        HP += hp;
        DrawHP();
    }
    public void SubMaxHP(int hp)
    {
        maxHP -= hp;
        HP = Mathf.Min(HP, maxHP);
        DrawHP();
    }
    public void AddHP(int hp)
    {
        HP += hp;
        HP = Mathf.Min(HP, maxHP);
        DrawHP();
    }
    void DrawHP()
    {
        for (int i = 0; i < HPImages.Length; i++)
        {
            if (maxHP < (i+1) * 2) { HPImages[i].SetActive(false); }
            else
            {
                HPImages[i].SetActive(true);
                HPImages[i].GetComponent<Animator>().SetInteger("HP", HP - (i) * 2);
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) { Damage(1); }
        if (Input.GetKeyDown(KeyCode.Delete)) { SubMaxHP(2); }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { AddHP(1); }
        if (Input.GetKeyDown(KeyCode.Return)) { AddMaxHP(2); }
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
        
        
        //позици€ курсора мыши
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
                if (currentClipInfo[0].clip.name == "SideShotAnim")//если это анимаци€ выстрела в бок
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
                if (currentClipInfo[0].clip.name == "DownShotAnim")//если это анимаци€ выстрела в низ
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
                if (currentClipInfo[0].clip.name == "SideShotAnim")//если это анимаци€ выстрела в бок
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
                if (currentClipInfo[0].clip.name == "DownShotAnim")//если это анимаци€ выстрела в низ
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
