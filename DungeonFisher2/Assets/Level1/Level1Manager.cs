using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level1Manager : MonoBehaviour
{
    public Tilemap level;
    public TileBase[] tiles;
    
    void Start()
    {
       
        Generator generator = new Generator();
        int[,] Dungeon = generator.Generate();
        for (int i = 0; i < Dungeon.GetLength(0); i++)
        {
            for (int j = 0; j < Dungeon.GetLength(1); j++)
            {
                if (Dungeon[i,j] == 1)
                {
                    level.SetTile(new Vector3Int(i, j, 0), tiles[0]);
                }
                if (Dungeon[i, j] == 3)
                {
                    level.SetTile(new Vector3Int(i, j, 0), tiles[1]);
                }
            }
        }
    }
    //
    //// Update is called once per frame
    //void Update()
    //{
    //    
    //}
}
