using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : Person
{
    
    public Player player;
    public List<Vector2Int> Path = new List<Vector2Int>();//временно публичный
    public float atackRadius;
    protected Vector2Int direction;
    public EnemyHitbox hitBox;
    
    public float rechargeTime;
    public float rechargeTimer;
    public LayerMask raycastLayer;

    public override void Start()
    {
        base.Start();
        rechargeTimer = rechargeTime;
        
    }
    public virtual void FixedUpdate()
    {
        int dungeonPoint = DetermPointInMatrix(); ;
        if (dungeonPoint == 0 || dungeonPoint == 2 || dungeonPoint == 5)
        {
            if (!isDead)
            {
                HP = 0;
                Dead();
            }
            rigidbody.velocity *= 0.8f;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), 0.1f);
            if (transform.localScale.x <= 0.25f) { Destroy(gameObject); }
            if (!isRipplesCreated)
            {
                ripplesObject = Instantiate(ripplesPrefab, transform.position, transform.rotation);


                isRipplesCreated = true;
            }
            else
            {
                if (ripplesObject != null) { ripplesObject.transform.position = transform.position; }
            }
        }
        else
        {
            transform.localScale = originalScale;
            isRipplesCreated = false;
            if (isDead && rigidbody.velocity == Vector2.zero) 
            { 
                animator.enabled = false;
                this.enabled = false;
            }
        }
    }
    
    protected void DetermDirection(Vector3 target)
    {
        Vector2 dir = (target - transform.position).normalized;
        int x = Mathf.RoundToInt(dir.x);
        int y = Mathf.RoundToInt(dir.y);
        if (Mathf.Abs(x) == Mathf.Abs(y))
        {
            // ¬ случае (1,1) или (-1,-1) уменьшаем большую компоненту до 0
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
    //protected void AStar()
    //{
    //    Path.Clear();
    //    Path.Add(new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt( player.transform.position.y)));
    //    //Debug.Log(Mathf.RoundToInt(player.transform.position.x) + " " + Mathf.RoundToInt(player.transform.position.y));
    //}
    protected void Move()
    {
        if (Path == null || Path.Count == 0) { return; }

        Vector2 targetPosition = ConvertMatrixCoordinateToPos(Path[0]); // ѕерва€ точка пути

        if (Vector2.Distance(targetPosition, transform.position) < 0.5f)
        {
            Path.RemoveAt(0);
        }

        if (Path.Count == 0) { return; }

        // –ассчитываем вектор смещени€ до следующей точки пути с учетом скорости
        Vector2 direction = (targetPosition - (Vector2)rigidbody.position).normalized;
        Vector2 movement = direction * moveSpeed * Time.deltaTime;

        // ѕримен€ем смещение к текущей позиции
        rigidbody.MovePosition(rigidbody.position + movement);
        DetermDirection(targetPosition);
    }
    protected void Move(Vector2 to)
    {
        Vector2 direction = (to - (Vector2)rigidbody.position).normalized;
        Vector2 movement = direction * moveSpeed * Time.deltaTime;

        // ѕримен€ем смещение к текущей позиции
        rigidbody.MovePosition(rigidbody.position + movement);
        DetermDirection(to);
    }

}
