using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Water2D;


public class Level1Manager : LevelManager
{
    Generator generator = new Generator();
    public ModernWater2D water;
    public Tilemap levelTilemap;
    public Tilemap wallsTilemap;
    public Tilemap briegesTilemap;
    public Tilemap reflectionsTilemap;
    public Player player;
    public GameObject playerPrefab;
    public GameObject[] HPImages;
    List<Generator.Room> AllRooms;
    
    public Texture2D fullMap;
    public Texture2D openedMap;
    [SerializeField]
    [Header("Floor Tiles")]
    Sprite[] CentralTiles;
    [SerializeField]
    float[] CentralWeights;
    
    [SerializeField]
    [Header("Wall Tiles")]
    Sprite LUCornerTile;
    [SerializeField]
    Sprite RUCornerTile;
    [SerializeField]
    Sprite LDCornerTile;
    [SerializeField]
    Sprite RDCornerTile;
    [SerializeField]
    Sprite LUConcaveTile;
    [SerializeField]
    Sprite RUConcaveTile;
    [SerializeField]
    Sprite RDConcaveTile;
    [SerializeField]
    Sprite LDConcaveTile;
    [SerializeField]
    Sprite LEdgeTile;
    [SerializeField]
    Sprite REdgeTile;
    [SerializeField]
    Sprite UEdgeTile;
    [SerializeField]
    Sprite DEdgeTile;

    [SerializeField]
    [Header("Reflection Tiles")]
    Sprite CentralReflection;
    [SerializeField]
    Sprite RCornerReflection;
    [SerializeField]
    Sprite LCornerReflection;
    [SerializeField]
    Sprite RConvareReflection;
    [SerializeField]
    Sprite LConvareReflection;
    [SerializeField]
    Sprite[] BriegesReflection;

    public Sprite Red;
    struct WallDir
    {
        public readonly int r;
        public readonly int l;
        public readonly int u;
        public readonly int d;

        public readonly int ru;
        public readonly int lu;
        public readonly int rd;
        public readonly int ld;
        public WallDir(int r,int l,int u,int d,int ru, int lu,int rd,int ld)
        {
            this.r = r;
            this.l = l;
            this.u = u;
            this.d = d;

            this.ru = ru;
            this.lu = lu;
            this.rd = rd;
            this.ld = ld;
        }
    }

    [SerializeField]
    [Header("Briedges")]
    Sprite[] HorBriedgeTiles;
    [SerializeField]
    Sprite[] VerBriedgeTiles;
    [SerializeField]
    float[] BridgesWeights;

    
    public GameObject tentaclesPrefab;
    public GameObject[] enemiesPrefabs;
    private int[] enemiesInDungeon = new int[1];//на данный момент типов противников - 1
    
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
    Vector2Int ChoiseEnemyPoint(Generator.Room room, ref int[,] roomMatrix)
    {
        Vector2Int point;
        do
        {
            point = new Vector2Int(Random.Range(0, room.RoomMatrix.GetLength(1)), Random.Range(0, room.RoomMatrix.GetLength(0)));
        }
        while (roomMatrix[point.y, point.x] != 1);
        roomMatrix[point.y, point.x] = 0;
        return point + new Vector2Int(room.FirstRoomPoint.X + 1, room.FirstRoomPoint.Y+1);
    }
    void CreateEnemy(int enemyNumber,Generator.Room room, ref int[,] roomMatrix)
    {
        enemies.Add(Instantiate(enemiesPrefabs[enemyNumber], (Vector2)ChoiseEnemyPoint(room,ref roomMatrix), Quaternion.identity));
        Enemies enemyScript = enemies[enemies.Count - 1].GetComponent<Enemies>();
        enemyScript.player = player;
        enemyScript.dungeon = dungeonMatrix;
    }
    void CheckOpeningMap()
    {
        Vector2Int playerPos = player.ConvertPosToMatrixCoordinate();
        if (!player.inBoat && player.DetermPointInMatrix(openedDungeonMatrix) == 0)
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
                    int[,] roomMatrix = (int[,])room.RoomMatrix.Clone();
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

                            CreateEnemy(i, room,ref roomMatrix);
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
        mapImage.uvRect = new Rect(new Vector2(((player.transform.position.x - 0.5f) / Generator.DUNGEON_SIDE_SIZE) - (mapImage.uvRect.width/2), ((player.transform.position.y - 0.5f) / Generator.DUNGEON_SIDE_SIZE) - (mapImage.uvRect.height / 2)), mapImage.uvRect.size);
        bossMarkMap.localPosition = ((bossRoomCenter / Generator.DUNGEON_SIDE_SIZE) -mapImage.uvRect.position)  * (mapImage.rectTransform.rect.size *2);
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
        for (int i = 0; i < enemiesInDungeon.Length; i++)
        {
            for (int e = 0; e < enemiesInDungeon[i]; e++)
            {
                Generator.Room randRoom = ChoiseRandomRoom(AllRooms);
                if (randRoom == null) { e -= 1; }
                else { randRoom.EnemiesQuantity[i] += 1; }
            }
        }
    }
    void WaterSetting()
    {
        Vector2[] waterSpeeds = new Vector2[]
        {
            new Vector2 (0.05f,0.025f),
            new Vector2 (0.25f,0.05f),
            new Vector2 (-0.25f,-0.05f),
            new Vector2 (-0.05f,-0.025f),
            new Vector2 (-0.05f,-0.05f),
            new Vector2 (0.05f,0.05f),
            new Vector2 (-0.05f,0.05f),
            new Vector2 (0.05f,-0.05f),
        };
        int num = Random.Range(0, waterSpeeds.Length);
        Vector2 waterSpeed = waterSpeeds[num];
        water.settings._waterSettings.distortionSpeed = new WaterCryo<Vector2>(waterSpeed);
        water.settings._waterSettings.foamSpeed = new WaterCryo<Vector2>(waterSpeed);
    }
    void Start()
    {
        enemiesInDungeon = new int[] { 40 };//////////////////это пока не добавили механику рыбалки
        WaterSetting();
        (int[,] dungeon, List<Generator.Room> allRooms,Vector2 playerStartPos,List<Vector2Int> PathToFishingPos, List<Vector2Int> PathToEndShipPos, Vector2Int shipDirection) = generator.Generate();
        dungeonMatrix = dungeon;
        AllRooms = allRooms;
        for (int i = 0; i < allRooms.Count; i++)
        {
            if (allRooms[i] is Generator.BossRoom) { bossRoomCenter = new Vector2((allRooms[i].FirstRoomPoint.X + allRooms[i].SecondRoomPoint.X)/2+1, (allRooms[i].FirstRoomPoint.Y + allRooms[i].SecondRoomPoint.Y) / 2+1); }
        }
        playerPrefab.GetComponent<Player>().startBoatDirection = shipDirection;
        playerPrefab.GetComponent<Player>().pathToFishingPoint = PathToFishingPos;
        playerPrefab.GetComponent<Player>().pathToEndPoint = PathToEndShipPos;
        playerPrefab.GetComponent<Player>().HPImages = HPImages;
        player = Instantiate(playerPrefab, playerStartPos, Quaternion.Euler(0, 0, 0)).GetComponent<Player>();
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
       
        Tile newTile = ScriptableObject.CreateInstance<Tile>();
        //newTile.colliderType = Tile.ColliderType.Grid;
        for (int i = 1; i < dungeon.GetLength(0)-1; i++)
        {
            for (int j = 1; j < dungeon.GetLength(1)-1; j++)
            {
                int x = j;
                int y = i;
                switch (dungeon[i, j])
                {
                    case 1:
                        newTile.sprite = ChooseRandomTile(CentralTiles, CentralWeights);//Center
                        levelTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                        break;
                    case 2:
                        WallDir tilesAround = new WallDir(dungeon[i, j + 1], dungeon[i, j - 1], dungeon[i + 1, j], dungeon[i - 1, j],
                            dungeon[i+1,j+1], dungeon[i + 1, j - 1], dungeon[i - 1, j + 1], dungeon[i - 1, j - 1]);//RLUD
                        switch (tilesAround)
                        {
                            case WallDir v when v.r == 1 && (v.u == 2 || v.u == 3) && (v.d == 2 || v.d == 3)://LeftEdge
                                newTile.sprite = LEdgeTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when v.l == 1 && (v.u == 2 || v.u == 3) && (v.d == 2 || v.d == 3)://RightEdge
                                newTile.sprite = REdgeTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when (v.r == 2 || v.r == 3) && (v.l == 2 || v.l == 3) && v.d == 1 ://UpEdge
                                newTile.sprite = UEdgeTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when (v.r == 2 || v.r == 3) && (v.l == 2 || v.l == 3) && v.u == 1://DownEdge
                                newTile.sprite = DEdgeTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                newTile.sprite = CentralReflection;
                                reflectionsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                        
                            case WallDir v when (v.l == 2 || v.l == 3) && (v.d == 2 || v.d == 3) && v.ld == 1://RUCorner
                                newTile.sprite = RUCornerTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when (v.l == 2 || v.l == 3) && (v.u == 2 || v.u == 3) && v.lu == 1://RDCorner
                                newTile.sprite = RDCornerTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                newTile.sprite = RCornerReflection;
                                reflectionsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when (v.r == 2 || v.r == 3) && (v.d == 2 || v.d == 3) && v.rd == 1://LUCorner
                                newTile.sprite = LUCornerTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when (v.r == 2 || v.r == 3) && (v.u == 2 || v.u == 3) && v.ru == 1://LDCorner
                                newTile.sprite = LDCornerTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                newTile.sprite = LCornerReflection;
                                reflectionsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                        
                            case WallDir v when v.l == 1 && v.d == 1 && v.ld == 1://RUConcave
                                newTile.sprite = RUConcaveTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when v.l == 1 && v.u == 1 && v.lu == 1://RDConcave
                                newTile.sprite = RDConcaveTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                newTile.sprite = RConvareReflection;
                                reflectionsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when v.r == 1 && v.d == 1 && v.rd == 1://LUConcave
                                newTile.sprite = LUConcaveTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                            case WallDir v when v.r == 1 && v.u == 1 && v.ru == 1://LDConcave
                                newTile.sprite = LDConcaveTile;
                                wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                newTile.sprite = LConvareReflection;
                                reflectionsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                                break;
                        }
                        break;
                    case 3:
                        if (dungeon[y, x + 1] == 3)
                        {
                            newTile.sprite = ChooseRandomTile(HorBriedgeTiles, BridgesWeights,out int num);
                            levelTilemap.SetTile(new Vector3Int(x, y, 1), newTile);
                            briegesTilemap.SetTile(new Vector3Int(x, y, 1), newTile);
                            if (num < BriegesReflection.Length)
                            {
                                newTile.sprite = BriegesReflection[num];
                                reflectionsTilemap.SetTile(new Vector3Int(x, y, 1), newTile);
                            }
                        }
                        if (dungeon[y + 1, x] == 3)
                        {
                            newTile.sprite = ChooseRandomTile(VerBriedgeTiles, BridgesWeights);
                            levelTilemap.SetTile(new Vector3Int(x, y, 1), newTile);
                            briegesTilemap.SetTile(new Vector3Int(x, y, 1), newTile);
                        }
                        break;
                    //case 5:
                    //    newTile.sprite = Red;
                    //    wallsTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                    //    break;
                }
            }
        }

    }

    Sprite  ChooseRandomTile(Sprite[] sprites, float[] weights)
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
    Sprite ChooseRandomTile(Sprite[] sprites, float[] weights, out int num)
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
                num = i;
                return sprites[i];
            }
        }
        num = sprites.Length - 1;
        return sprites[sprites.Length - 1];
    }
    Generator.Room ChoiseRandomRoom(List<Generator.Room> rooms)
    {
        float totalWeight = 0f;
        for (int i = 1; i < rooms.Count; i++)
        {
            if (rooms[i] is Generator.BossRoom) { continue; }
            totalWeight += rooms[i].roomSize;
        }
        float randomValue = Random.Range(0f, totalWeight);// Генерируем случайное число от 0 до суммы весов

        float cumulativeWeight = 0f;
        for (int i = 1; i < rooms.Count; i++)
        {
            if (rooms[i] is Generator.BossRoom) { continue; }
            cumulativeWeight += rooms[i].roomSize;
            if (randomValue <= cumulativeWeight)
            {
                return rooms[i];
            }
        }
        return null;
    }
}
