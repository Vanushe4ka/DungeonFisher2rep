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
    

    public float rechargeTime;
    protected float rechargeTimer;
    void Start()
    {
        rechargeTimer = rechargeTime;
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
    }
    public void Damage(int damage)
    {
        HP -= damage;
        StartCoroutine(Blink());
        if (HP <= 0) { Destroy(gameObject); }
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
