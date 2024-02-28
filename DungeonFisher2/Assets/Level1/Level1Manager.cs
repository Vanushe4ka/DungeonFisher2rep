using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Level1Manager : LevelManager
{
    Generator generator = new Generator();
    public Tilemap level;
    public TileBase[] tiles;
    int[,] dungeonMatrix;
    int[,] openedDungeonMatrix = new int[100,100];
    public Player player; 
    List<Generator.Room> AllRooms;
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
        Vector2 point = new Vector2(Random.Range(0,room.RoomMatrix.GetLength(1)), Random.Range(0, room.RoomMatrix.GetLength(0)));
        while (room.RoomMatrix[(int)point.y, (int)point.x] != 1)
        {
           point = new Vector2(Random.Range(0, room.RoomMatrix.GetLength(1)), Random.Range(0, room.RoomMatrix.GetLength(0)));
        }
       ///Debug.Log(point.x + " " + point.y + " - " + (point.x + room.FirstRoomPoint.X) + " " + (point.y + room.FirstRoomPoint.Y));
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
        Vector2Int playerPos = new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.y)-1);
       ///Debug.Log(openedDungeonMatrix[playerPos.x, playerPos.y]);
        if (openedDungeonMatrix[playerPos.y, playerPos.x] == 0)
        {
            //Debug.Log("CheckRoomList");
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
                            if (openedDungeonMatrix[i,j] == 0) { openedMap.SetPixel(j, i, Color.white); }
                            else if(openedDungeonMatrix[i, j] == 1) { openedMap.SetPixel(j, i, Color.black); }
                            else { openedMap.SetPixel(j, i, Color.green); }

                        }
                    }
                    openedMap.SetPixel(playerPos.x, playerPos.y, Color.red);
                    openedMap.SetPixel(playerPos.x+1, playerPos.y, Color.red);
                    openedMap.SetPixel(playerPos.x+1, playerPos.y+1, Color.red);
                    openedMap.SetPixel(playerPos.x, playerPos.y+1, Color.red);
                    openedMap.SetPixel(playerPos.x-1, playerPos.y, Color.red);
                    openedMap.SetPixel(playerPos.x, playerPos.y-1, Color.red);
                    openedMap.SetPixel(playerPos.x-1, playerPos.y-1, Color.red);
                    openedMap.SetPixel(playerPos.x-1, playerPos.y+1, Color.red);
                    openedMap.SetPixel(playerPos.x+1, playerPos.y-1, Color.red);
                    openedMap.Apply();
                }
            }
        }
    }
    private void FixedUpdate()
    {
        CheckOpeningMap();
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
        player.dungeon = dungeon;
        player.levelManager = this;
        openedMap = new Texture2D(100, 100);
        fullMap = new Texture2D(100, 100);
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
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] == 1)
                    {
                        newTile.sprite = ChooseRandomTile(CentralTiles, CentralWeights);
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    

                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] != 1)//LU
                    {
                        newTile.sprite = LUCornerTiles[Random.Range(0, LUCornerTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    
                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] == 1)//RU
                    {
                        newTile.sprite = RUCornerTiles[Random.Range(0, RUCornerTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }

                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] != 1)//LD
                    {
                        newTile.sprite = LDCornerTiles[Random.Range(0, LDCornerTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] == 1)//RD
                    {
                        newTile.sprite = RDCornerTiles[Random.Range(0, RDCornerTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    
                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] == 1)//hor
                    {
                        newTile.sprite = HorCoridorTiles[Random.Range(0, HorCoridorTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] != 1)//ver
                    {
                        newTile.sprite = VerCoridorTiles[Random.Range(0, VerCoridorTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] != 1)//L edge
                    {
                        newTile.sprite = LEdgeTiles[Random.Range(0, LEdgeTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] == 1)//R edge
                    {
                        newTile.sprite = REdgeTiles[Random.Range(0, REdgeTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }

                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] == 1)//U edge
                    {
                        newTile.sprite = UEdgeTiles[Random.Range(0, UEdgeTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] == 1)//D edge
                    {
                        newTile.sprite = DEdgeTiles[Random.Range(0, DEdgeTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    
                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] == 1 && dungeon[i, j - 1] != 1)//L peninsula
                    {
                        newTile.sprite = LPeninsulaTiles[Random.Range(0, LPeninsulaTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    
                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] == 1)//R peninsula
                    {
                        newTile.sprite = RPeninsulaTiles[Random.Range(0, RPeninsulaTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }

                    if (dungeon[i + 1, j] != 1 && dungeon[i - 1, j] == 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] != 1)//U peninsula
                    {
                        newTile.sprite = UPeninsulaTiles[Random.Range(0, UPeninsulaTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    if (dungeon[i + 1, j] == 1 && dungeon[i - 1, j] != 1 && dungeon[i, j + 1] != 1 && dungeon[i, j - 1] != 1)//D peninsula
                    {
                        newTile.sprite = DPeninsulaTiles[Random.Range(0, DPeninsulaTiles.Length)];
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    //level.SetTile(new Vector3Int(i, j, 0), tiles[0]);
                }
                if (dungeon[i, j] == 3)
                {
                    if (dungeon[y,x+1] == 3)
                    {
                        newTile.sprite = ChooseRandomTile(HorBriedgeTiles, BridgesWeights);
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    if (dungeon[y+1, x] == 3)
                    {
                        newTile.sprite = ChooseRandomTile(VerBriedgeTiles, BridgesWeights);
                        level.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    //level.SetTile(new Vector3Int(i, j, 0), tiles[1]);
                }
            }
        }

    }

    Sprite ChooseRandomTile(Sprite[] sprites, float[] weights)
    {
        // ¬ычисл€ем сумму всех весов
        

        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        // √енерируем случайное число в диапазоне от 0 до суммы всех весов
        float randomValue = Random.Range(0f, totalWeight);

        // ¬ыбираем изображение на основе случайного числа и весов
        float cumulativeWeight = 0f;
        for (int i = 0; i < sprites.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return sprites[i];
            }
        }

        // ¬озвращаем последнее изображение, если не удалось выбрать изображение
        return sprites[sprites.Length - 1];
    }
}
