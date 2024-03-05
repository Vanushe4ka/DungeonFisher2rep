using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    public Rigidbody2D rigidbody;
    public SpriteRenderer spriteRenderer;
    protected Animator animator;
    public int HP;
    public float moveSpeed;
    protected Vector2 originalScale;
    public bool isDead;

    public int[,] dungeon;
    protected bool isRipplesCreated;
    public GameObject ripplesPrefab;
    protected GameObject ripplesObject;
    public virtual void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        originalScale = transform.localScale;
    }
    public int DetermPointInMatrix()
    {
        return dungeon[Mathf.RoundToInt(transform.position.y) - 1, Mathf.RoundToInt(transform.position.x) - 1];
    }
    public int DetermPointInMatrix(int [,] dungeon)
    {
        return dungeon[Mathf.RoundToInt(transform.position.y) - 1, Mathf.RoundToInt(transform.position.x) - 1];
    }
    public Vector2Int ConvertPosToMatrixCoordinate()
    {
        return new Vector2Int(Mathf.RoundToInt(gameObject.transform.position.x) - 1, Mathf.RoundToInt(gameObject.transform.position.y) - 1);
    }
    public Vector2Int ConvertMatrixCoordinateToPos(Vector2Int matrixCoordinate)
    {
        return new Vector2Int(matrixCoordinate.x +1,matrixCoordinate.y+1);
    }
    public virtual void Dead()
    {
        isDead = true;
        animator.SetBool("dead", true);
        gameObject.layer = 10;
        StartCoroutine(ChangeColorBlackout());
    }
    public virtual void Dead(Vector3 pushVector, float pushForce)
    {
        Dead();
        Vector3 direction = (transform.position - pushVector).normalized;
        rigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }
    public virtual void Damage(int hp)
    {
        HP -= hp;
        HP = Mathf.Max(HP, 0);
        StartCoroutine(Blink());
    }
    public virtual void CalculateLayer()
    {
        spriteRenderer.sortingOrder = 400 - Mathf.RoundToInt(transform.position.y * 4);
    }
    protected IEnumerator ChangeColorBlackout()
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
    protected IEnumerator Blink()
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
}
