using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Level1Manager : LevelManager
{
    Generator generator = new Generator();
    public Tilemap level;
    
    
    public Player player; 
    List<Generator.Room> AllRooms;
    public Vector2 bossRoomCenter;//временно публичный
    public Texture2D fullMap;
    public Texture2D openedMap;
    [SerializeField]
    [Header("Tiles")]
    Sprite[] CentralTiles;
    [SerializeField]
    float[] CentralWeights;
    [SerializeField]
    Sprite[] LUCornerTiles;
    [SerializeField]
    Sprite[] RUCornerTiles;
    [SerializeField]
    Sprite[] LDCornerTiles;
    [SerializeField]
    Sprite[] RDCornerTiles;
    [SerializeField]
    Sprite[] HorCoridorTiles;
    [SerializeField]
    Sprite[] VerCoridorTiles;

    [SerializeField]
    Sprite[] LEdgeTiles;
    [SerializeField]
    Sprite[] REdgeTiles;
    [SerializeField]
    Sprite[] UEdgeTiles;
    [SerializeField]
    Sprite[] DEdgeTiles;
    [SerializeField]
    Sprite[] RPeninsulaTiles;
    [SerializeField]
    Sprite[] LPeninsulaTiles;
    [SerializeField]
    Sprite[] UPeninsulaTiles;
    [SerializeField]
    Sprite[] DPeninsulaTiles;
    [SerializeField]
    [Header("Briedges")]
    Sprite[] HorBriedgeTiles;
    [SerializeField]
    Sprite[] VerBriedgeTiles;
    [SerializeField]
    float[] BridgesWeights;

    
    public GameObject tentaclesPrefab;
    public GameObject[] enemiesPrefabs;
    private List<Vector2Int> pointsWereEnemies = new List<Vector2Int>(); 
    void AddTentacles(Generator.Room room)
    {
        for (int y = room.FirstRoomPoint.Y; y <= room.SecondRoomPoint.Y; y++)
        {
            for (int x = room.FirstRoomPoint.X; x <= room.SecondRoomPoint.X; x++)
            {
                if (dungeonMatrix[y,x] == 3)
                {
                    //tentacles.Add(Instantiate(tentaclesPrefab, new Vector2(x, y), new Quaternion(0, 0, 0, 0)));
                    if (dungeonMatrix[y-1,x] == 1)
                    {
                        tentacles.Add(Instantiate(tentaclesPrefab, new Vector2(x+1, y + 1.8f), new Quaternion(0, 0, 0, 0)));
                        tentacles[tentacles.Count - 1].GetComponent<Animator>().SetBool("isHorisontal", true);
                    }
                    else if (dungeonMatrix[y + 1, x] == 1)
                    {
                        tentacles.Add(Instantiate(tentaclesPrefab, new Vector2(x+1, y + 0.8f), new Quaternion(0, 0, 0, 0)));
                        tentacles[tentacles.Count - 1].GetComponent<Animator>().SetBool("isHorisontal", true);
                    }
                    else if (dungeonMatrix[y, x-1] == 1)
                    {
                        tentacles.Add(Instantiate(tentaclesPrefab, new Vector2(x + 1.5f, y+1.3f), new Quaternion(0, 0, 0, 0)));
                        
                    }
                    else if (dungeonMatrix[y, x+1] == 1)
                    {
                        tentacles.Add(Instantiate(tentaclesPrefab, new Vector2(x + 0.5f, y+1.3f), new Quaternion(0, 0, 0, 0)));
                        
                    }
                }
            }
        }
    }
    Vector2 ChoiseEnemyPoint(Generator.Room room)
    {
        Vector2Int point;
        do
        {
            point = new Vector2Int(Random.Range(0, room.RoomMatrix.GetLength(1)), Random.Range(0, room.RoomMatrix.GetLength(0)));
        }
        while (room.RoomMatrix[point.y, point.x] != 1 && !pointsWereEnemies.Contains(point));
        pointsWereEnemies.Add(point);
        return point + new Vector2(room.FirstRoomPoint.X + 1, room.FirstRoomPoint.Y+1);
    }
    void CreateEnemy(int enemyNumber,Generator.Room room)
    {
        enemies.Add(Instantiate(enemiesPrefabs[enemyNumber], ChoiseEnemyPoint(room), Quaternion.identity));
        Enemies enemyScript = enemies[enemies.Count - 1].GetComponent<Enemies>();
        enemyScript.player = player;
        enemyScript.dungeon = dungeonMatrix;
    }
    void CheckOpeningMap()
    {
        Vector2Int playerPos = player.ConvertPosToMatrixCoordinate();
        if (player.DetermPointInMatrix(openedDungeonMatrix) == 0)
        {
            foreach(Generator.Room room in generator.AllRooms)
            {
                //Debug.Log("matrixSize - " + room.RoomMatrix.GetLength(0) + "*" + room.RoomMatrix.GetLength(1));
                if (room.isVisited == true) { continue; }
                Vector2Int roomPos = new Vector2Int(room.FirstRoomPoint.X, room.FirstRoomPoint.Y);
                Vector2Int playerPosInRoom = playerPos - roomPos;
                //Debug.Log(playerPosInRoom.x + " " + playerPosInRoom.y);
                
                if (playerPosInRoom.x >= 0 && playerPosInRoom.y >= 0 && playerPosInRoom.x < room.RoomMatrix.GetLength(1) && playerPosInRoom.y < room.RoomMatrix.GetLength(0)&&
                     room.RoomMatrix[playerPosInRoom.y,playerPosInRoom.x] == 1)
                {
                    
                    
                    room.isVisited = true;
                    for (int i = 0; i < room.EnemiesQuantity.Length; i++)
                    {
                        for (int e = 0; e < room.EnemiesQuantity[i]; e++)
                        {
                            //Debug.Log("Entered Room - " + room.GetType());
                            if (!isFight)
                            {
                                isFight = true;
                                AddTentacles(room);
                            }

                            CreateEnemy(i, room);
                        }
                    }
                    pointsWereEnemies.Clear();

                    for (int i = 0; i < room.RoomMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < room.RoomMatrix.GetLength(1); j++)
                        {
                            if (openedDungeonMatrix[room.FirstRoomPoint.Y + i, room.FirstRoomPoint.X + j] == 0)
                            {                                                                      
                                openedDungeonMatrix[room.FirstRoomPoint.Y + i, room.FirstRoomPoint.X + j] = room.RoomMatrix[i, j];
                            }
                            //Debug.Log((room.FirstRoomPoint.Y + i) + " " + (room.FirstRoomPoint.X + j));
                        }
                    }
                    for (int i = 0; i < openedDungeonMatrix.GetLength(0); i++)
                    {
                        for (int j=0;j < openedDungeonMatrix.GetLength(1); j++)
                        {
                            if(openedDungeonMatrix[i, j] == 1) 
                            {
                                if (i > 0 && i < openedDungeonMatrix.GetLength(0) && j > 0 && j < openedDungeonMatrix.GetLength(1) &&
                                    openedDungeonMatrix[i+1, j] == 1 && openedDungeonMatrix[i - 1, j] == 1 && openedDungeonMatrix[i, j+1] == 1 && openedDungeonMatrix[i, j-1] == 1)
                                {
                                    openedMap.SetPixel(j, i, new Color(0.8901f, 0.7843f, 0.7019f, Random.Range(0.5f, 1f)));
                                }
                                else
                                {
                                    openedMap.SetPixel(j, i, new Color(0.5725f, 0.4823f, 0.4901f, Random.Range(0.5f, 1f)));
                                }
                            }
                            else if (openedDungeonMatrix[i, j] == 3)
                            {
                                openedMap.SetPixel(j, i, Color.black);
                            }
                            else { openedMap.SetPixel(j, i, new Color(0, 0, 0, 0)); }

                        }
                    }
                    openedMap.Apply();
                }
            }
        }
    }
    private void MapHandler()
    {
        mapImage.uvRect = new Rect(new Vector2(((player.transform.position.x - 0.5f) / Generator.DUNGEON_SIZE) - (mapImage.uvRect.width/2), ((player.transform.position.y - 0.5f) / Generator.DUNGEON_SIZE) - (mapImage.uvRect.height / 2)), mapImage.uvRect.size);
        bossMarkMap.localPosition = ((bossRoomCenter / Generator.DUNGEON_SIZE) -mapImage.uvRect.position)  * (mapImage.rectTransform.rect.size *2);
    }
    private void FixedUpdate()
    {
        CheckOpeningMap();
        MapHandler();
        
        if (isFight)
        {
            //enemies.RemoveAll(item => item == null);
            for (int i =0;i<enemies.Count;i++)
            {
                if (enemies[i] == null || enemies[i].GetComponent<Enemies>().isDead == true) { enemies.RemoveAt(i);i--; }
            }
            if (enemies.Count == 0)
            {
                foreach(GameObject tentacle in tentacles)
                {
                    tentacle.GetComponent<Animator>().SetBool("isClosed", false);
                }
                tentacles.Clear();
                isFight = false;
            }
        }
    }
    void DistributionEnemies()
    {
        foreach (Generator.Room room in generator.AllRooms)
        {
            if (room is Generator.StartRoom == false) { room.EnemiesQuantity = new int[] { 2 }; }
        }
    }
    void Start()
    {
        
        
        (int[,] dungeon, List<Generator.Room> allRooms) = generator.Generate();
        for (int i = 0; i < allRooms.Count; i++)
        {
            if (allRooms[i] is Generator.BossRoom) { bossRoomCenter = new Vector2((allRooms[i].FirstRoomPoint.X + allRooms[i].SecondRoomPoint.X)/2+1, (allRooms[i].FirstRoomPoint.Y + allRooms[i].SecondRoomPoint.Y) / 2+1); }
        }
        player.dungeon = dungeon;
        player.levelManager = this;
        openedMap = new Texture2D(100, 100);
        openedMap.filterMode = FilterMode.Point;
        
        fullMap = new Texture2D(100, 100);
        mapImage.texture = openedMap;
        
        DistributionEnemies();
        for (int i = 0; i < dungeon.GetLength(0); i++)
        {
            for (int j = 0; j < dungeon.GetLength(1); j++)
            {
                if (dungeon[i, j] == 0) { fullMap.SetPixel(j, i, Color.white); }
                else if (dungeon[i, j] == 1) { fullMap.SetPixel(j, i, Color.black); }
                else { fullMap.SetPixel(j, i, Color.green); }

            }
        }
        fullMap.Apply();
        openedDungeonMatrix = new int[dungeon.GetLength(0), dungeon.GetLength(1)];
        dungeonMatrix = dungeon;
        AllRooms = allRooms;

        Tile newTile = ScriptableObject.CreateInstance<Tile>();
        for (int i = 1; i < dungeon.GetLength(0)-1; i++)
        {
            for (int j = 1; j < dungeon.GetLength(1)-1; j++)
            {
                int x = j;
                int y = i;
                if (dungeon[i,j] == 1)
                {
                    Vector4 tilesAround = new Vector4(dungeon[i + 1, j], dungeon[i - 1, j], dungeon[i, j + 1], dungeon[i, j - 1]);//RLUD
                    switch (tilesAround)
                    {
                        case Vector4 v when v.x == 1 && v.y == 1 && v.z == 1 && v.w == 1:
                            newTile.sprite = ChooseRandomTile(CentralTiles, CentralWeights);//Center
                            break;
                        case Vector4 v when v.x != 1 && v.y == 1 && v.z == 1 && v.w != 1:
                            newTile.sprite = LUCornerTiles[Random.Range(0, LUCornerTiles.Length)];//LU
                            break;
                        case Vector4 v when v.x != 1 && v.y == 1 && v.z != 1 && v.w == 1:
                            newTile.sprite = RUCornerTiles[Random.Range(0, RUCornerTiles.Length)];//RU
                            break;
                        case Vector4 v when v.x == 1 && v.y != 1 && v.z == 1 && v.w != 1:
                            newTile.sprite = LDCornerTiles[Random.Range(0, LDCornerTiles.Length)];//LD
                            break;
                        case Vector4 v when v.x == 1 && v.y != 1 && v.z != 1 && v.w == 1:
                            newTile.sprite = RDCornerTiles[Random.Range(0, RDCornerTiles.Length)];//RD
                            break;
                        case Vector4 v when v.x != 1 && v.y != 1 && v.z == 1 && v.w == 1:
                            newTile.sprite = HorCoridorTiles[Random.Range(0, HorCoridorTiles.Length)];//hor
                            break;
                        case Vector4 v when v.x == 1 && v.y == 1 && v.z != 1 && v.w != 1:
                            newTile.sprite = VerCoridorTiles[Random.Range(0, VerCoridorTiles.Length)];//ver
                            break;
                        case Vector4 v when v.x == 1 && v.y == 1 && v.z == 1 && v.w != 1:
                            newTile.sprite = LEdgeTiles[Random.Range(0, LEdgeTiles.Length)];//L edge
                            break;
                        case Vector4 v when v.x == 1 && v.y == 1 && v.z != 1 && v.w == 1:
                            newTile.sprite = REdgeTiles[Random.Range(0, REdgeTiles.Length)];//R edge
                            break;
                        case Vector4 v when v.x != 1 && v.y == 1 && v.z == 1 && v.w == 1:
                            newTile.sprite = UEdgeTiles[Random.Range(0, UEdgeTiles.Length)];//U edge
                            break;
                        case Vector4 v when v.x == 1 && v.y != 1 && v.z == 1 && v.w == 1:
                            newTile.sprite = DEdgeTiles[Random.Range(0, DEdgeTiles.Length)];//D edge
                            break;
                        case Vector4 v when v.x != 1 && v.y != 1 && v.z == 1 && v.w != 1:
                            newTile.sprite = LPeninsulaTiles[Random.Range(0, LPeninsulaTiles.Length)];//L peninsula
                            break;
                        case Vector4 v when v.x != 1 && v.y != 1 && v.z == 1 && v.w == 1:
                            newTile.sprite = RPeninsulaTiles[Random.Range(0, RPeninsulaTiles.Length)];//R peninsula
                            break;
                        case Vector4 v when v.x != 1 && v.y == 1 && v.z != 1 && v.w != 1:
                            newTile.sprite = UPeninsulaTiles[Random.Range(0, UPeninsulaTiles.Length)];//U peninsula
                            break;
                        case Vector4 v when v.x == 1 && v.y != 1 && v.z != 1 && v.w != 1:
                            newTile.sprite = DPeninsulaTiles[Random.Range(0, DPeninsulaTiles.Length)];//D peninsula
                            break;
                    }
                    level.SetTile(new Vector3Int(x, y, 0), newTile);
                }
                if (dungeon[i, j] == 3)
                {
                    if (dungeon[y,x+1] == 3)
                    {
                        newTile.sprite = ChooseRandomTile(HorBriedgeTiles, BridgesWeights);
                        level.SetTile(new Vector3Int(x, y, 1), newTile);
                    }
                    if (dungeon[y+1, x] == 3)
                    {
                        newTile.sprite = ChooseRandomTile(VerBriedgeTiles, BridgesWeights);
                        level.SetTile(new Vector3Int(x, y, 1), newTile);
                    }
                    
                    //level.SetTile(new Vector3Int(i, j, 0), tiles[1]);
                }
            }
        }

    }

    Sprite ChooseRandomTile(Sprite[] sprites, float[] weights)
    {
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }
        float randomValue = Random.Range(0f, totalWeight);// Генерируем случайное число от 0 до суммы весов

        float cumulativeWeight = 0f;
        for (int i = 0; i < sprites.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return sprites[i];
            }
        }
        return sprites[sprites.Length - 1];
    }
}
