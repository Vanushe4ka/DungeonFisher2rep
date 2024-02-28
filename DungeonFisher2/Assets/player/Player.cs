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

    public bool isDead;
    private Vector3 originalScale;
    public int[,] dungeon;
    public LevelManager levelManager;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        originalScale = transform.localScale;
        DrawHP();
    }
    public void CalculateLayer()
    {
        spriteRenderer.sortingOrder = 400 - Mathf.RoundToInt(transform.position.y*4);
        if (ActiveGun != null) { ActiveGun.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; }
    }
    public void Damage(int hp)
    {
        HP -= hp;
        HP = Mathf.Max(HP, 0);
        StartCoroutine(Blink());
        DrawHP();
    }
    public void Dead(Vector3 pushVector, float pushForce)
    {
        Dead();
        Vector3 direction = (transform.position - pushVector).normalized;
        rigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }
    public void Dead()
    {
        levelManager.PlayerIsDead();
        isDead = true;
        animator.SetBool("dead", true);
        gameObject.layer = 10;
        StartCoroutine(ChangeColorBlackout());
    }
    private IEnumerator ChangeColorBlackout()
    {
        float elapsedTime = 0f;
        float duration = 1;
        Color originalColor = spriteRenderer.color;
        originalColor.a = 1;
        Color targetColor = new Color(originalColor.r - 0.5f, originalColor.g - 0.5f, originalColor.b - 0.5f, originalColor.a);
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            spriteRenderer.color = Color.Lerp(originalColor, targetColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = targetColor; // ”бедитьс€, что цвет стал точно целевым
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
        if (!isDead)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -10), cameraSpeed);
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
                
                Quaternion rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
                ActiveGun.transform.rotation = Quaternion.Slerp(ActiveGun.transform.rotation, rotation, ActiveGun.rotationSpeed * Time.deltaTime);

                animator.SetInteger("direction", 1);
                rotateDirection = new Vector2Int(0, 1);
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
            else if (angle <= -45 && angle > -135)
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
        else
        {
            int dungeonPoint = dungeon[Mathf.RoundToInt(transform.position.y) - 1, Mathf.RoundToInt(transform.position.x) - 1];
            if (dungeonPoint == 0 || dungeonPoint == 2 || dungeonPoint == 5)
            {
                rigidbody.velocity *= 0.8f;
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), 0.1f);
                //if (transform.localScale.x <= 0.25f) { Destroy(gameObject); }

            }
            else
            {
                transform.localScale = originalScale;
            }
        }
    }

}
