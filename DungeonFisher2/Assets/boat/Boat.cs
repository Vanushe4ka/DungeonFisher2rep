using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Water2D;
using TMPro;

public class Boat : Person
{
    public List<Player> playersInBoat = new List<Player>();
    public Player localPlayer;
    public Vector2[] startPlayerPoints;
    public Vector2Int direction;
    public PolygonCollider2D[] collidersDependingDirection;

    public Obstructor boatObstructor;
    private float[] obstuctorValues = new float[] { 1f, 0.57f, 0.57f, 1f };
    public GameObject obstructoObj;

    public bool isFishing = true;

    public float maxSpeed;
    public float minSpeed;
    public float accelerationRate;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> pathToFishingPoint = new List<Vector2Int>();
    bool isMoving;
    [SerializeField] private float maxDistanceToMessage;
    [SerializeField] private float minDistanceToMessage;
    IEnumerator writeMessageCorutine;
    [SerializeField] bool isWriting;
    [SerializeField] private Text messageText;
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private Vector2 canvasStartPos;

    [SerializeField] Camera mainCam;

    public override void Start()
    {
        base.Start();

        mainCam = Camera.main;
        canvasStartPos = canvasTransform.localPosition;
        boatObstructor = gameObject.GetComponent<Obstructor>();
        ChangeDirection(direction);
        ChangeDistancesToMessage(0, 1.5f);
    }
    public void AnimFrameChange(float swingDistance)
    {
        if (playersInBoat.Count > 0)
        {
            for(int i = 0; i < playersInBoat.Count; i++)
            {
                playersInBoat[i].transform.position += new Vector3(0, swingDistance, 0);
            }
            
        }
    }
    private void ChangeDistancesToMessage(float min,float max)
    {
        minDistanceToMessage = min;
        maxDistanceToMessage = max;
    }
    public void PlayerSeat(Player player)
    {
        playersInBoat.Add(player);
        player.transform.position = transform.position + (Vector3)startPlayerPoints[ConvertDirToInt4(direction)];
        player.inBoat = true;
    }
    public void PlayerDiseat(Player player)
    {
        
        playersInBoat.Remove(player);
        player.transform.position = (Vector2)ConvertMatrixCoordinateToPos(PathFinder.ChoisePointNear(player.ConvertPosToMatrixCoordinate(), player.dungeon,new List<int> { 1}));
        player.inBoat = false;
    }
    public void ChangeDirection(Vector2Int newDir)
    {
        int newDirInt = ConvertDirToInt4(newDir);
        if (newDirInt < collidersDependingDirection.Length)
        {
            for (int i = 0; i < collidersDependingDirection.Length; i++) { collidersDependingDirection[i].enabled = false; }
            collidersDependingDirection[newDirInt].enabled = true;
        }
            

        if (playersInBoat.Count > 0) { RotateAroundPoint(transform.position, direction, newDir); }
        Vector3 offset = new Vector3(0,0);
        direction = newDir;
        if (direction.x == 1) { spriteRenderer.flipX = true; reflectionRenderer.flipX = true; }
        else { spriteRenderer.flipX = false; reflectionRenderer.flipX = false; }
        animator.SetInteger("direction", newDirInt);
        boatObstructor.height = obstuctorValues[newDirInt];
        boatObstructor.CreateData();
        obstructoObj.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
    }
    public void RotateAroundPoint(Vector3 point, Vector2Int initialDirection, Vector2Int newDirection)
    {
        // Вычисляем смещение относительно точки
        for(int i = 0; i < playersInBoat.Count; i++)
        {
            Vector3 offset = playersInBoat[i].transform.position - point;

            // Вычисляем угол между текущим и новым направлениями
            float angle = AngleBetweenDirections(initialDirection, newDirection);

            // Поворачиваем объект относительно точки на угол
            playersInBoat[i].transform.position = RotateVectorAroundPoint(offset, angle) + point;
        }
        
    }
    private float AngleBetweenDirections(Vector2Int directionA, Vector2Int directionB)
    {
        float angle = Vector2.SignedAngle(directionA, directionB);
        return angle;
    }
    private Vector3 RotateVectorAroundPoint(Vector3 vector, float angle)
    {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);

        // Поворачиваем вектор
        float newX = vector.x * cos - vector.y * sin;
        float newY = vector.x * sin + vector.y * cos;

        return new Vector3(newX, newY, vector.z);
    }
    
    // Update is called once per frame
    private void Move()
    {
        List<Vector2Int> Path;
        if (isFishing) {Path = pathToFishingPoint; }
        else { Path = path; }
        isMoving = Path.Count != 0;
        if (Path.Count > 0)
        {
            if (Path.Count == 1 && Vector2.Distance(Path[0],transform.position) < 10) {moveSpeed = Mathf.Lerp(moveSpeed, minSpeed, Time.fixedDeltaTime * accelerationRate); }
            else { moveSpeed = Mathf.Lerp(moveSpeed, maxSpeed, Time.fixedDeltaTime * accelerationRate); }
            Vector2 direction = (Path[0] - (Vector2)transform.position).normalized;
            float distance = Vector3.Distance(transform.position, (Vector2)Path[0]);
            if (distance > 0.25f)
            {
                // Вычисляем дельту позиции
                Vector2 DeltaPos = direction * Mathf.Min(moveSpeed * Time.fixedDeltaTime, distance);

                transform.position += (Vector3)DeltaPos;
                if (playersInBoat.Count > 0)
                {
                    for(int i = 0; i < playersInBoat.Count; i++)
                    {
                        playersInBoat[i].transform.position += (Vector3)DeltaPos;
                    }
                }
            }
            else
            {
                Path.RemoveAt(0);
                if (Path.Count > 0)
                {
                    DetermDirection((Vector2)Path[0]);
                }
            }
        }
        else
        {
            moveSpeed = 0;
        }
    }
    void FixedUpdate()
    {
        Move();
        if (!isMoving)
        {
            float distanceToMessage = Vector2.Distance(transform.position, localPlayer.transform.position);
            if (distanceToMessage < maxDistanceToMessage && distanceToMessage > minDistanceToMessage)
            {
                if (!isWriting)
                {
                    isWriting = true;
                    StartCoroutine(writeMessageCorutine = WriteMessage(DetermMessage()));
                }
                Bounds cameraBounds = UpdateCameraBounds();
                
                Vector2 newPosition = (Vector2)transform.position + canvasStartPos;
                // Ограничиваем позицию никнейма в пределах границ камеры
                newPosition.x = Mathf.Clamp(newPosition.x, cameraBounds.min.x + 1.5f, cameraBounds.max.x - 1.5f);
                newPosition.y = Mathf.Clamp(newPosition.y, cameraBounds.min.y + 1.5f, cameraBounds.max.y - 1.5f);
                // Применяем новую позицию к никнейму
                canvasTransform.transform.position = newPosition;
            }
            else
            {
                if (writeMessageCorutine!= null) { StopCoroutine(writeMessageCorutine); }
                isWriting = false;
                messageText.text = "";
            }
        }
    }
    private string DetermMessage()
    {
        if (isFishing) { return "нажмите [E] чтобы плыть к островам"; }
        else
        {
            if (localPlayer.inBoat) { return "нажмите [E] чтобы высадиться"; }
            else { return "нажмите [E] чтобы сесть в лодку"; }
        }
    }
    private IEnumerator WriteMessage(string message)
    {
        messageText.text = "";
        for (int i = 0; i < message.Length; i++)
        {

            messageText.text = message.Substring(0,i);
            yield return new WaitForSeconds(0.05f);
            
        }
        messageText.text = message;
        
    }
    protected void DetermDirection(Vector3 target)
    {
        Vector2 dir = (target - transform.position).normalized;
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
        ChangeDirection( new Vector2Int(x, y));
    }
    Bounds UpdateCameraBounds()
    {
        // Получаем границы камеры в мировых координатах
        Vector3 lowerLeft = mainCam.ScreenToWorldPoint(Vector3.zero);
        Vector3 upperRight = mainCam.ScreenToWorldPoint(new Vector3(mainCam.pixelWidth, mainCam.pixelHeight, 0));

        // Создаем новый объект Bounds на основе границ камеры
        return new Bounds((lowerLeft + upperRight) * 0.5f, upperRight - lowerLeft);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad8)) { ChangeDirection(new Vector2Int(0, 1)); }
        else if (Input.GetKeyDown(KeyCode.Keypad2)) { ChangeDirection(new Vector2Int(0, -1)); }
        else if (Input.GetKeyDown(KeyCode.Keypad4)) { ChangeDirection(new Vector2Int(-1, 0)); }
        else if (Input.GetKeyDown(KeyCode.Keypad6)) { ChangeDirection(new Vector2Int(1, 0)); }
        
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            if (isWriting)
            {
                

                if (!isFishing)
                {
                    if (localPlayer.inBoat)
                    {
                        PlayerDiseat(localPlayer);
                    }
                    else { PlayerSeat(localPlayer); }
                }
                else { isFishing = false; ChangeDistancesToMessage(2,6.5f);}


                isWriting = false;
                if (writeMessageCorutine != null)
                {
                    StopCoroutine(writeMessageCorutine);
                }
                messageText.text = "";
            }
            
        }

    }
}
