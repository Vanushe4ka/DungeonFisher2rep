using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level1Manager : MonoBehaviour
{
    public Tilemap level;
    public TileBase[] tiles;
    int[,] dungeonMatrix;
    List<Generator.Room> AllRooms;

    
    void Start()
    {
        Generator generator = new Generator();
        (int[,] dungeon, List<Generator.Room> allRooms) = generator.Generate();
        dungeonMatrix = dungeon;
        AllRooms = allRooms;
        for (int i = 0; i < dungeon.GetLength(0); i++)
        {
            for (int j = 0; j < dungeon.GetLength(1); j++)
            {
                if (dungeon[i,j] == 1)
                {
                    level.SetTile(new Vector3Int(i, j, 0), tiles[0]);
                }
                if (dungeon[i, j] == 3)
                {
                    level.SetTile(new Vector3Int(i, j, 0), tiles[1]);
                }
            }
        }

    }
    
}
