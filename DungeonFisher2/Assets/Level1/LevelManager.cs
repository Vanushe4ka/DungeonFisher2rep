using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public bool isFight = false;
    protected List<GameObject> enemies = new List<GameObject>();
    public List<GameObject> tentacles;
    public RawImage mapImage;
    public RectTransform playerMarkMap;
    public RectTransform bossMarkMap;

    protected int[,] dungeonMatrix;
    public int[,] openedDungeonMatrix = new int[Generator.DUNGEON_SIZE, Generator.DUNGEON_SIZE];
    public void PlayerIsDead()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].GetComponent<Enemies>().rechargeTimer = float.MaxValue;
        }
        foreach (GameObject tentacle in tentacles)
        {
            tentacle.GetComponent<Animator>().SetBool("isClosed", false);
        }
        tentacles.Clear();
        isFight = false;
    }
    public int GetPointValueInOpenedMatrix(Vector2Int point)
    {
        return openedDungeonMatrix[point.y, point.x];
    }
}
