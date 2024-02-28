using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public Rigidbody2D rigidbody;
    public SpriteRenderer spriteRenderer;
    protected Animator animator;
    public int HP;
    public float moveSpeed;
    public Player player;
    protected List<Vector2Int> Path = new List<Vector2Int>();
    public float atackRadius;
    protected Vector2Int direction;
    public EnemyHitbox hitBox;
    public int[,] dungeon;

    public bool isDead;
    public float rechargeTime;
    protected float rechargeTimer;

    private Vector2 originalScale;
    void Start()
    {
        rechargeTimer = rechargeTime;
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        originalScale = transform.localScale;
    }
    public virtual void FixedUpdate()
    {
        if (isDead)
        {
            int dungeonPoint = dungeon[Mathf.RoundToInt(transform.position.y) - 1, Mathf.RoundToInt(transform.position.x) - 1];
            if (dungeonPoint == 0 || dungeonPoint == 2 || dungeonPoint == 5)
            {
                rigidbody.velocity *= 0.8f;
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), 0.1f);
                if (transform.localScale.x <= 0.25f) { Destroy(gameObject); }

            }
            else
            {
                transform.localScale = originalScale;
            }
        }
    }
    public void Damage(int damage)
    {
        HP -= damage;
        StartCoroutine(Blink());
        
    }
    public virtual void Dead(Vector3 pushVector,float pushForce)
    {
        isDead = true;
        animator.SetBool("dead", true);
        gameObject.layer = 10;
        StartCoroutine(ChangeColorBlackout());
        Vector3 direction = (transform.position - pushVector).normalized;
        rigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }
    public void CalculateLayer()
    {
        spriteRenderer.sortingOrder = 400- Mathf.RoundToInt(transform.position.y*4);
    }
    
    protected void DetermDirection()
    {
        Vector2 dir = (player.transform.position - transform.position).normalized;
        int x = Mathf.RoundToInt(dir.x);
        int y = Mathf.RoundToInt(dir.y);
        if (Mathf.Abs(x) == Mathf.Abs(y))
        {
            // В случае (1,1) или (-1,-1) уменьшаем большую компоненту до 0
            if (y != 0)
            {
                x = 0;
            }
            else
            {
                y = 0;
            }
        }
        //Debug.Log(x + "-" + y);
        direction = new Vector2Int(x, y);
    }
    public virtual void Atack() { }
    protected void AStar()
    {
        Path.Clear();
        Path.Add(new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt( player.transform.position.y)));
        //Debug.Log(Mathf.RoundToInt(player.transform.position.x) + " " + Mathf.RoundToInt(player.transform.position.y));
    }
    protected void Move()
    {
        if (Path.Count == 0) { return; }

        Vector2 targetPosition = new Vector2(Path[0].x, Path[0].y); // Первая точка пути

        if (Vector2.Distance(targetPosition, transform.position) < 1)
        {
            Path.RemoveAt(0);
        }

        if (Path.Count == 0) { return; }

        // Рассчитываем вектор смещения до следующей точки пути с учетом скорости
        Vector2 direction = (targetPosition - (Vector2)rigidbody.position).normalized;
        Vector2 movement = direction * moveSpeed * Time.deltaTime;

        // Применяем смещение к текущей позиции
        rigidbody.MovePosition(rigidbody.position + movement);
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
        spriteRenderer.color = targetColor; // Убедиться, что цвет стал точно целевым
    }
    IEnumerator Blink()
    {
        float elapsedTime = 0f;
        float blinkDuration = 0.15f;
        while (elapsedTime < blinkDuration)
        {
            float t = Mathf.Cos(elapsedTime / blinkDuration*2 * Mathf.PI); // Интерполяция синуса от 0 до PI
            float alpha = t/8 + 0.75f; // Ограничение альфа не менее minAlpha

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            elapsedTime += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }
        
        // Установить альфа обратно на 1 в конце анимации
        Color endColor = spriteRenderer.color;
        endColor.a = 1f;
        spriteRenderer.color = endColor;
    }
    
}
