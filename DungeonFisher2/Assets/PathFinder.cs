using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    private class PointAStat
    {
        public readonly float function;//F = lengthFormStart + lengthToEnd
        public int lengthFromStart;
        public Vector2Int point;
        public PointAStat prewPoint;
        public PointAStat(int length,Vector2Int point,PointAStat prewPoint,Vector2Int endPoint)
        {
            lengthFromStart = length;
            function = length + Vector2Int.Distance(point,endPoint);
            this.point = point;
            this.prewPoint = prewPoint;
        }
    }
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int to, int[,] matrix, List<int> pointsToMove)
    {
        if (from == to) { return null; }
        List <PointAStat> pointsForChecks = new List<PointAStat>(); 
        List <Vector2Int> checkedPoints = new List<Vector2Int>();
        pointsForChecks.Add(new PointAStat(0, from, null,to));
        float AverageFunction = pointsForChecks[0].function;
        while (pointsForChecks.Count > 0)
        {
            PointAStat curentPoint = pointsForChecks[pointsForChecks.Count - 1];
            for (int i = pointsForChecks.Count - 1; i >= 0; i--)
            {
                if (pointsForChecks[i].function < curentPoint.function) { curentPoint = pointsForChecks[i];AverageFunction = pointsForChecks[i].function; }
                if (pointsForChecks[i].function <= AverageFunction) { AverageFunction = pointsForChecks[i].function; break; }
            }

            if (curentPoint.point.x > 0) 
            {
                Vector2Int lp = new Vector2Int(curentPoint.point.x - 1, curentPoint.point.y);
                if (lp == to) { return BuildPath(new PointAStat(curentPoint.lengthFromStart + 1, lp, curentPoint, to)); }
                if (pointsToMove.Contains(matrix[lp.y,lp.x]) && !checkedPoints.Contains(lp)) { pointsForChecks.Add(new PointAStat(curentPoint.lengthFromStart + 1, lp, curentPoint, to)); }
            }
            if (curentPoint.point.x < Generator.DUNGEON_SIDE_SIZE) 
            {
                Vector2Int rp = new Vector2Int(curentPoint.point.x + 1, curentPoint.point.y);
                if (rp == to) { return BuildPath(new PointAStat(curentPoint.lengthFromStart + 1, rp, curentPoint, to)); }
                if (pointsToMove.Contains(matrix[rp.y, rp.x]) && !checkedPoints.Contains(rp)) { pointsForChecks.Add(new PointAStat(curentPoint.lengthFromStart + 1, rp, curentPoint, to)); }
            }
            if (curentPoint.point.y > 0) 
            {
                Vector2Int dp = new Vector2Int(curentPoint.point.x, curentPoint.point.y-1);
                if (dp == to) { return BuildPath(new PointAStat(curentPoint.lengthFromStart + 1, dp, curentPoint, to)); }
                if (pointsToMove.Contains(matrix[dp.y, dp.x]) && !checkedPoints.Contains(dp)) { pointsForChecks.Add(new PointAStat(curentPoint.lengthFromStart + 1, dp, curentPoint, to)); }
            }
            if (curentPoint.point.y < Generator.DUNGEON_SIDE_SIZE) 
            {
                Vector2Int up = new Vector2Int(curentPoint.point.x, curentPoint.point.y + 1);
                if (up == to) { return BuildPath(new PointAStat(curentPoint.lengthFromStart + 1, up, curentPoint, to)); }
                if (pointsToMove.Contains(matrix[up.y, up.x]) && !checkedPoints.Contains(up)) { pointsForChecks.Add(new PointAStat(curentPoint.lengthFromStart + 1, up, curentPoint, to)); }
            }
            checkedPoints.Add(curentPoint.point);
            pointsForChecks.Remove(curentPoint);
        }
        return null;
    }
    private static List<Vector2Int> BuildPath(PointAStat endPoint)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        while(endPoint.prewPoint != null)
        {
            result.Add(endPoint.point);
            endPoint = endPoint.prewPoint;
        }
        result.Reverse();
        return result;
    }

    public static Vector2Int ChoisePointNear(Vector2Int from, int[,] matrix, List<int> pointsToCoise)
    {
        List<Vector2Int> pointsForChecks = new List<Vector2Int>();
        List<Vector2Int> checkedPoints = new List<Vector2Int>();
        pointsForChecks.Add(from);
        while (pointsForChecks.Count > 0)
        {
            if (pointsToCoise.Contains(matrix[pointsForChecks[0].y, pointsForChecks[0].x])) { return pointsForChecks[0]; }
            else
            {
                if (pointsForChecks[0].x > 0)
                {
                    pointsForChecks.Add(new Vector2Int(pointsForChecks[0].x - 1,pointsForChecks[0].y));
                }
                if (pointsForChecks[0].x < Generator.DUNGEON_SIDE_SIZE)
                {
                    pointsForChecks.Add(new Vector2Int(pointsForChecks[0].x + 1, pointsForChecks[0].y));
                }
                if (pointsForChecks[0].y > 0)
                {
                    pointsForChecks.Add(new Vector2Int(pointsForChecks[0].x, pointsForChecks[0].y - 1));
                }
                if (pointsForChecks[0].y < Generator.DUNGEON_SIDE_SIZE)
                {
                    pointsForChecks.Add(new Vector2Int(pointsForChecks[0].x, pointsForChecks[0].y + 1));
                }
                pointsForChecks.RemoveAt(0);
            }
        }
        return new Vector2Int(Generator.DUNGEON_SIDE_SIZE/2, Generator.DUNGEON_SIDE_SIZE/2);
    }
}
