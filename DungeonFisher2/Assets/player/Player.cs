using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Person
{
    // Start is called before the first frame update
    public float animMoveSpeed;
    Vector2Int rotateDirection = new Vector2Int(0, -1);
    public Transform cameraTransform;
    public float cameraSpeed;
    public Gun[] AllGuns;
    public Gun ActiveGun;

    public int maxHP;
    public GameObject[] HPImages;

    public LevelManager levelManager;
    private bool isFellToWater;

    private Vector2Int LastPosition;
    private IEnumerator currentShakeCoroutine;
    public override void Start()
    {
        base.Start();
        for(int i = 0; i < AllGuns.Length; i++) { AllGuns[i].player = this; }
        DrawHP();
    }
    public override void CalculateLayer()
    {
        base.CalculateLayer();
        if (ActiveGun != null) { ActiveGun.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; }
    }
    public override void Damage(int hp)
    {
        base.Damage(hp);
        ShakeCamera(hp/10f,0.1f);
        DrawHP();
    }
    public void ShakeCamera(float magnitude, float duration)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }
        currentShakeCoroutine = ShakeCoroutine(magnitude, duration);
        StartCoroutine(currentShakeCoroutine);
    }
    private IEnumerator ShakeCoroutine(float magnitude, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 randomPoint = cameraTransform.position + Random.insideUnitSphere * magnitude;
            cameraTransform.localPosition = randomPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }
        //transform.localPosition = originalPosition;
    }
    public override void Dead()
    {
        levelManager.PlayerIsDead();
        base.Dead();
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
    public bool IsCanPlay()
    {
        return (!isDead && !isFellToWater);
    }
   
    void  FixedUpdate()
    {
        CalculateLayer();
        int dungeonPoint = DetermPointInMatrix();
        if (dungeonPoint == 0 || dungeonPoint == 2 || dungeonPoint == 5)
        {
            if (HP == 1) { Damage(1);Dead(); }
            else if (!isDead && HP > 1)
            {
                isFellToWater = true;
            }
            if (!isRipplesCreated)
            {
                ripplesObject = Instantiate(ripplesPrefab, transform.position, transform.rotation);
                isRipplesCreated = true;
            }
            else
            {
                if (ripplesObject != null) { ripplesObject.transform.position = transform.position; }
            }
            rigidbody.velocity *= 0.8f;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), 0.1f);
            if (isFellToWater && transform.localScale.x < 0.1f)
            {
                Debug.Log("StartFindingPoint");
                gameObject.transform.position = (Vector2)ConvertMatrixCoordinateToPos(LastPosition);
                Debug.Log("EndFindingPoint");
                isFellToWater = false;
                Damage(1);
            }
            //if (transform.localScale.x <= 0.25f) { Destroy(gameObject); }

        }
        else
        {
            transform.localScale = originalScale;
            isRipplesCreated = false;
            isFellToWater = false;
            LastPosition = ConvertPosToMatrixCoordinate();
        }

        if (IsCanPlay())
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
            else if (angle <= -45 && angle > -135)
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

}
