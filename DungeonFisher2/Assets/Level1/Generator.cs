using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
public class Generator
{
    public const int DUNGEON_SIZE = 100;
    const int MIN_WALL_SIZE = 6;
    const int MAX_WALL_SIZE = 15;
    //const int MinRoomsQuantity = 6;
    //const int MaxRoomsQuantity = 10;
    static bool thereBossRoom;
    const int MIN_DUNGEON_SIZE = 900;
    const int MAX_DUNGEON_SIZE = 1100;
    static int thisDungeonSize;
    public List<Room> AllRooms = new List<Room>();
    public struct Point
    {
        public int X { get; }
        public int Y { get; }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        public static bool operator ==(Point a, Point b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }
        public static bool operator !=(Point a, Point b)
        {
            return !((a.X == b.X) && (a.Y == b.Y));
        }
    }
    public class BossRoom : Room
    {
        public BossRoom(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
            : base(entranceDoor, firstRoomPoint, secondRoomPoint, dungeon, allRooms, random) { }
        public override void DrawRoom()
        {
            FreeDirections.Clear();
            base.DrawRoom();
        }
    }
    public class StartRoom : Room
    {
        public StartRoom(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
            : base(entranceDoor, firstRoomPoint, secondRoomPoint, dungeon, allRooms, random) { }
        public override void DrawRoom()
        {
            base.DrawRoom();
            Point shipDirection = FreeDirections[Random.Next(0, FreeDirections.Count)];
            FreeDirections.Remove(shipDirection);
            int shipWidth = 6;
            if (shipDirection.X != 0)
            {
                int fillWidth = 0;
                int startFillPoint;
                if (shipDirection.X == 1) { startFillPoint = SecondRoomPoint.X + shipDirection.X; }
                else { startFillPoint = FirstRoomPoint.X + shipDirection.X; }
                while (fillWidth < shipWidth && fillWidth > -shipWidth)
                {
                    for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
                    {
                        Dungeon[i, startFillPoint + fillWidth] = 5;
                    }
                    fillWidth += shipDirection.X;
                }
            }
            else if (shipDirection.Y != 0)
            {
                int fillWidth = 0;
                int startFillPoint;
                if (shipDirection.Y == 1) { startFillPoint = SecondRoomPoint.Y + shipDirection.Y; }
                else { startFillPoint = FirstRoomPoint.Y + shipDirection.Y; }
                while (fillWidth < shipWidth && fillWidth > -shipWidth)
                {
                    for (int i = FirstRoomPoint.X; i <= SecondRoomPoint.X; i++)
                    {
                        Dungeon[startFillPoint + fillWidth, i] = 5;
                    }
                    fillWidth += shipDirection.Y;
                }
            }
        }
    }
    public class CircleRoom : Room
    {
        public CircleRoom(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
            : base(entranceDoor, firstRoomPoint, secondRoomPoint, dungeon, allRooms, random) { }
        public override void DrawRoom()
        {
            int a = Math.Abs(SecondRoomPoint.X - FirstRoomPoint.X) / 2; // Полуось по оси X
            int b = Math.Abs(SecondRoomPoint.Y - FirstRoomPoint.Y) / 2; // Полуось по оси Y
            int centerX = (SecondRoomPoint.X + FirstRoomPoint.X) / 2; // Центр эллипса по оси X
            int centerY = (SecondRoomPoint.Y + FirstRoomPoint.Y) / 2; // Центр эллипса по оси Y

            for (int y = FirstRoomPoint.Y; y <= SecondRoomPoint.Y; y++)
            {
                for (int x = FirstRoomPoint.X; x <= SecondRoomPoint.X; x++)
                {
                    double distance = Math.Pow(x - centerX, 2) / Math.Pow(a, 2) + Math.Pow(y - centerY, 2) / Math.Pow(b, 2);
                    if (Math.Abs(1 - distance) < 0.01) // Значение 1 означает границу эллипса
                    {
                        Dungeon[y, x] = 2; // Граница - стена
                    }
                    else if (distance < 1) // Значение меньше 1 означает внутреннюю часть эллипса
                    {
                        Dungeon[y, x] = 1; // Внутренняя часть
                    }
                }
            }
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    if (Dungeon[i, j] == 0) { thisDungeonSize -= 1; continue; }
                    if (Dungeon[i, j] == 1)
                    {
                        if (i + 1 < DUNGEON_SIZE && Dungeon[i + 1, j] == 0) { Dungeon[i + 1, j] = 2; }
                        if (i - 1 >= 0 && Dungeon[i - 1, j] == 0) { Dungeon[i - 1, j] = 2; }
                        if (j + 1 < DUNGEON_SIZE && Dungeon[i, j + 1] == 0) { Dungeon[i, j + 1] = 2; }
                        if (j - 1 >= 0 && Dungeon[i, j - 1] == 0) { Dungeon[i, j - 1] = 2; }
                    }

                }
            }
        }
    }
    public class ThreeQuarterRoom : Room
    {
        Point EnteranceDoor;
        List<(Point, Point)> DirDoorPos = new List<(Point, Point)>();
        public ThreeQuarterRoom(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
               : base(entranceDoor, firstRoomPoint, secondRoomPoint, dungeon, allRooms, random)
        {
            EnteranceDoor = (Point)entranceDoor;
            DrawRoom();
        }
        public override void AddRoom(Point? thisDirection = null, Point? thisDoorPos = null)
        {
            if (FreeDirections.Count == 0) { return; }
            thisDirection = FreeDirections[Random.Next(0, FreeDirections.Count)];
            List<(Point, Point)> doorPoss = new List<(Point, Point)>();
            //Console.WriteLine("cikle");
            for (int i = 0; i < DirDoorPos.Count; i++)
            {
                (Point dir, Point doorPos) = DirDoorPos[i];
                Point testDir = (Point)thisDirection;
                //Console.WriteLine("dir: " + dir.X + " " + dir.Y + " testDir: " + testDir.X + " " + testDir.Y);
                if (dir == (Point)thisDirection)
                {
                    doorPoss.Add((dir, doorPos));
                }
            }
            if (doorPoss.Count == 0) { return; }
            (Point direction, Point doorPosition) = doorPoss[Random.Next(0, doorPoss.Count)];
            thisDirection = direction;
            thisDoorPos = doorPosition;
            //Console.WriteLine(direction.X + " " + direction.Y + " | " + doorPosition.X + " " + doorPosition.Y);
            base.AddRoom(direction, doorPosition);

        }
        public override void DrawRoom()
        {
            if (EnteranceDoor == new Point(0, 0)) { return; }
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    Dungeon[i, j] = 1;
                    Dungeon[FirstRoomPoint.Y, j] = 2;
                    Dungeon[SecondRoomPoint.Y, j] = 2;
                }
                Dungeon[i, FirstRoomPoint.X] = 2;
                Dungeon[i, SecondRoomPoint.X] = 2;
            }
            bool LU = false;
            bool LD = false;
            bool RU = false;
            bool RD = false;

            int randomVariations = Random.Next(0, 2);
            //RD = true;
            //Console.WriteLine(EnteranceDoor.X + " | " + EnteranceDoor.Y);
            if (EnteranceDoor.X == 1 && EnteranceDoor.Y == 0) { if (randomVariations == 0) { LU = true; } else { LD = true; } }
            else if (EnteranceDoor.X == -1 && EnteranceDoor.Y == 0) { if (randomVariations == 0) { RU = true; } else { RD = true; } }
            else if (EnteranceDoor.X == 0 && EnteranceDoor.Y == 1) { if (randomVariations == 0) { LU = true; } else { RU = true; } }
            else if (EnteranceDoor.X == 0 && EnteranceDoor.Y == -1) { if (randomVariations == 0) { LD = true; } else { RD = true; } }

            thisDungeonSize -= (((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2)) * ((SecondRoomPoint.X - FirstRoomPoint.X) / 2);

            if (LU)
            {
                for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y - ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2); i++)
                {
                    for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X - ((SecondRoomPoint.X - FirstRoomPoint.X) / 2); j++)
                    {
                        Dungeon[i, j] = 0;
                        //Dungeon[FirstRoomPoint.Y, j] = 2;
                        Dungeon[SecondRoomPoint.Y - ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2), j] = 2;
                    }
                    //Dungeon[i, FirstRoomPoint.X] = 2;
                    Dungeon[i, SecondRoomPoint.X - ((SecondRoomPoint.X - FirstRoomPoint.X) / 2)] = 2;
                }
            }
            if (LD)
            {
                for (int i = FirstRoomPoint.Y + ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2); i <= SecondRoomPoint.Y; i++)
                {
                    for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X - ((SecondRoomPoint.X - FirstRoomPoint.X) / 2); j++)
                    {
                        Dungeon[i, j] = 0;
                        Dungeon[FirstRoomPoint.Y + ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2), j] = 2;
                        //Dungeon[SecondRoomPoint.Y, j] = 2;
                    }
                    //Dungeon[i, FirstRoomPoint.X] = 2;
                    Dungeon[i, SecondRoomPoint.X - ((SecondRoomPoint.X - FirstRoomPoint.X) / 2)] = 2;
                }
            }
            if (RU)
            {
                for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y - ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2); i++)
                {
                    for (int j = FirstRoomPoint.X + ((SecondRoomPoint.X - FirstRoomPoint.X) / 2); j <= SecondRoomPoint.X; j++)
                    {
                        Dungeon[i, j] = 0;
                        //Dungeon[FirstRoomPoint.Y, j] = 2;
                        Dungeon[SecondRoomPoint.Y - ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2), j] = 2;
                    }
                    Dungeon[i, FirstRoomPoint.X + ((SecondRoomPoint.X - FirstRoomPoint.X) / 2)] = 2;
                    //Dungeon[i, SecondRoomPoint.X] = 2;
                }

            }
            if (RD)
            {
                for (int i = FirstRoomPoint.Y + ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2); i <= SecondRoomPoint.Y; i++)
                {
                    for (int j = FirstRoomPoint.X + ((SecondRoomPoint.X - FirstRoomPoint.X) / 2); j <= SecondRoomPoint.X; j++)
                    {
                        Dungeon[i, j] = 0;
                        Dungeon[FirstRoomPoint.Y + ((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2), j] = 2;
                        //Dungeon[SecondRoomPoint.Y, j] = 2;
                    }
                    Dungeon[i, FirstRoomPoint.X + ((SecondRoomPoint.X - FirstRoomPoint.X) / 2)] = 2;
                    //Dungeon[i, SecondRoomPoint.X] = 2;
                }

            }
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    if (Dungeon[i, j] == 2)
                    {
                        if (i + 1 < DUNGEON_SIZE && i - 1 >= 0 && Dungeon[i + 1, j] == 2 && Dungeon[i - 1, j] == 2 && i + 2 < DUNGEON_SIZE && i - 2 >= 0 && Dungeon[i + 2, j] == 2 && Dungeon[i - 2, j] == 2)
                        {
                            if (j + 1 < DUNGEON_SIZE && Dungeon[i, j + 1] == 1)
                            {
                                DirDoorPos.Add((new Point(-1, 0), new Point(j, i)));
                                continue;
                            }
                            if (j - 1 > 0 && Dungeon[i, j - 1] == 1)
                            {
                                DirDoorPos.Add((new Point(1, 0), new Point(j, i)));
                                continue;
                            }
                        }
                        if (j + 1 < DUNGEON_SIZE && j - 1 >= 0 && Dungeon[i, j + 1] == 2 && Dungeon[i, j - 1] == 2 && j + 2 < DUNGEON_SIZE && j - 2 >= 0 && Dungeon[i, j + 2] == 2 && Dungeon[i, j - 2] == 2)
                        {
                            if (i + 1 < DUNGEON_SIZE && Dungeon[i + 1, j] == 1)
                            {
                                DirDoorPos.Add((new Point(0, -1), new Point(j, i)));
                                continue;
                            }
                            if (i - 1 > 0 && Dungeon[i - 1, j] == 1)
                            {
                                DirDoorPos.Add((new Point(0, 1), new Point(j, i)));
                                continue;
                            }
                        }
                    }
                }
            }
            RoomMatrix = new int[SecondRoomPoint.Y - FirstRoomPoint.Y + 1, SecondRoomPoint.X - FirstRoomPoint.X + 1];
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    RoomMatrix[i - FirstRoomPoint.Y, j - FirstRoomPoint.X] = Dungeon[i, j];
                }
            }
        }
    }
    public class CrossRoom : Room
    {
        public CrossRoom(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
               : base(entranceDoor, firstRoomPoint, secondRoomPoint, dungeon, allRooms, random)
        {
        }
        public override void DrawRoom()
        {
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    Dungeon[i, j] = 1;
                    Dungeon[FirstRoomPoint.Y, j] = 2;
                    Dungeon[SecondRoomPoint.Y, j] = 2;
                }
                Dungeon[i, FirstRoomPoint.X] = 2;
                Dungeon[i, SecondRoomPoint.X] = 2;
            }
            int xDellThickness = Random.Next(1, Math.Max(1, (int)Math.Round((double)((SecondRoomPoint.X - FirstRoomPoint.X) / 2 - 1))));
            int yDellThickness = Random.Next(1, Math.Max(1, (int)Math.Round((double)((SecondRoomPoint.Y - FirstRoomPoint.Y) / 2 - 1))));
            thisDungeonSize -= xDellThickness * yDellThickness * 4;
            for (int i = FirstRoomPoint.Y; i <= FirstRoomPoint.Y + yDellThickness; i++)
            {
                for (int j = FirstRoomPoint.X; j <= FirstRoomPoint.X + xDellThickness; j++)
                {
                    Dungeon[i, j] = 0;
                    Dungeon[FirstRoomPoint.Y + yDellThickness, j] = 2;
                }
                Dungeon[i, FirstRoomPoint.X + xDellThickness] = 2;
            }
            for (int i = SecondRoomPoint.Y; i >= SecondRoomPoint.Y - yDellThickness; i--)
            {
                for (int j = FirstRoomPoint.X; j <= FirstRoomPoint.X + xDellThickness; j++)
                {
                    Dungeon[i, j] = 0;
                    Dungeon[SecondRoomPoint.Y - yDellThickness, j] = 2;
                }
                Dungeon[i, FirstRoomPoint.X + xDellThickness] = 2;
            }
            for (int i = FirstRoomPoint.Y; i <= FirstRoomPoint.Y + yDellThickness; i++)
            {
                for (int j = SecondRoomPoint.X; j >= SecondRoomPoint.X - xDellThickness; j--)
                {
                    Dungeon[i, j] = 0;
                    Dungeon[FirstRoomPoint.Y + yDellThickness, j] = 2;
                }
                Dungeon[i, SecondRoomPoint.X - xDellThickness] = 2;
            }
            for (int i = SecondRoomPoint.Y; i >= SecondRoomPoint.Y - yDellThickness; i--)
            {
                for (int j = SecondRoomPoint.X; j >= SecondRoomPoint.X - xDellThickness; j--)
                {
                    Dungeon[i, j] = 0;
                    Dungeon[SecondRoomPoint.Y - yDellThickness, j] = 2;
                }
                Dungeon[i, SecondRoomPoint.X - xDellThickness] = 2;
            }

        }
    }
    public class CristalRoom : Room
    {
        public CristalRoom(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
               : base(entranceDoor, firstRoomPoint, secondRoomPoint, dungeon, allRooms, random)
        {
        }
        public override void DrawRoom()
        {
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    Dungeon[i, j] = 1;
                    Dungeon[FirstRoomPoint.Y, j] = 2;
                    Dungeon[SecondRoomPoint.Y, j] = 2;
                }
                Dungeon[i, FirstRoomPoint.X] = 2;
                Dungeon[i, SecondRoomPoint.X] = 2;
            }
            int xDellThickness = (SecondRoomPoint.X - FirstRoomPoint.X);//Random.Next(Math.Max(1, (SecondRoomPoint.Y - FirstRoomPoint.Y) / 4 - 1), Math.Max(1, (SecondRoomPoint.X - FirstRoomPoint.X) / 2 - 1));
            int yDellThickness = (SecondRoomPoint.Y - FirstRoomPoint.Y);//Random.Next(Math.Max(1, (SecondRoomPoint.Y - FirstRoomPoint.Y) / 4 - 1), Math.Max(1, (SecondRoomPoint.Y - FirstRoomPoint.Y) / 2 - 1));
            int ChangedXDellThinkess = Math.Min(xDellThickness, yDellThickness);
            for (int i = FirstRoomPoint.Y; i <= FirstRoomPoint.Y + yDellThickness; i++)
            {
                for (int j = FirstRoomPoint.X; j <= FirstRoomPoint.X + ChangedXDellThinkess; j++)
                {
                    Dungeon[i, j] = 0;
                    ChangedXDellThinkess -= 1;// (float)xDellThickness / (float)yDellThickness;
                                              //Dungeon[FirstRoomPoint.Y + yDellThickness, j] = 2;
                }
                //Dungeon[i, FirstRoomPoint.X + xDellThickness] = 2;
            }
            ChangedXDellThinkess = Math.Min(xDellThickness, yDellThickness);
            for (int i = SecondRoomPoint.Y; i >= SecondRoomPoint.Y - yDellThickness; i--)
            {
                for (int j = FirstRoomPoint.X; j <= FirstRoomPoint.X + ChangedXDellThinkess; j++)
                {
                    Dungeon[i, j] = 0;
                    ChangedXDellThinkess -= 1;//(float)xDellThickness / (float)yDellThickness;
                                              //Dungeon[SecondRoomPoint.Y - yDellThickness, j] = 2;
                }
                //Dungeon[i, FirstRoomPoint.X + xDellThickness] = 2;
            }
            ChangedXDellThinkess = Math.Min(xDellThickness, yDellThickness);
            for (int i = FirstRoomPoint.Y; i <= FirstRoomPoint.Y + yDellThickness; i++)
            {
                for (int j = SecondRoomPoint.X; j >= SecondRoomPoint.X - ChangedXDellThinkess; j--)
                {
                    Dungeon[i, j] = 0;
                    ChangedXDellThinkess -= 1;//(float)xDellThickness / (float)yDellThickness;
                                              //Dungeon[FirstRoomPoint.Y + yDellThickness, j] = 2;
                }
                //Dungeon[i, SecondRoomPoint.X - xDellThickness] = 2;
            }
            ChangedXDellThinkess = Math.Min(xDellThickness, yDellThickness);
            //Console.WriteLine("\nDrawing Process: ");
            for (int i = SecondRoomPoint.Y; i >= SecondRoomPoint.Y - yDellThickness; i--)
            {
                for (int j = SecondRoomPoint.X; j >= SecondRoomPoint.X - ChangedXDellThinkess; j--)
                {
                    Dungeon[i, j] = 0;
                    //Console.WriteLine("ChangedXDellThinkess: " + ChangedXDellThinkess);
                    ChangedXDellThinkess -= 1;//(float)xDellThickness / (float)yDellThickness;
                                              //Dungeon[SecondRoomPoint.Y - yDellThickness, j] = 2;
                }
                //Dungeon[i, SecondRoomPoint.X - xDellThickness] = 2;
            }
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    if (Dungeon[i, j] == 0) { thisDungeonSize -= 1; continue; }
                    if (Dungeon[i, j] == 1)
                    {
                        if (i + 1 < DUNGEON_SIZE && Dungeon[i + 1, j] == 0) { Dungeon[i + 1, j] = 2; }
                        if (i - 1 >= 0 && Dungeon[i - 1, j] == 0) { Dungeon[i - 1, j] = 2; }
                        if (j + 1 < DUNGEON_SIZE && Dungeon[i, j + 1] == 0) { Dungeon[i, j + 1] = 2; }
                        if (j - 1 >= 0 && Dungeon[i, j - 1] == 0) { Dungeon[i, j - 1] = 2; }
                    }

                }
            }
            Dungeon[FirstRoomPoint.Y, (FirstRoomPoint.X + SecondRoomPoint.X) / 2 + 1] = 2;
            Dungeon[FirstRoomPoint.Y, (FirstRoomPoint.X + SecondRoomPoint.X) / 2 - 1] = 2;
            Dungeon[SecondRoomPoint.Y, (FirstRoomPoint.X + SecondRoomPoint.X) / 2 + 1] = 2;
            Dungeon[SecondRoomPoint.Y, (FirstRoomPoint.X + SecondRoomPoint.X) / 2 - 1] = 2;

            Dungeon[(FirstRoomPoint.Y + SecondRoomPoint.Y) / 2 + 1, FirstRoomPoint.X] = 2;
            Dungeon[(FirstRoomPoint.Y + SecondRoomPoint.Y) / 2 - 1, FirstRoomPoint.X] = 2;
            Dungeon[(FirstRoomPoint.Y + SecondRoomPoint.Y) / 2 + 1, SecondRoomPoint.X] = 2;
            Dungeon[(FirstRoomPoint.Y + SecondRoomPoint.Y) / 2 - 1, SecondRoomPoint.X] = 2;

        }
    }

    public class Room
    {
        public bool isVisited = false;
        public int[] EnemiesQuantity = new int[1];
        public List<Point> FreeDirections = new List<Point> { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };
        public Point FirstRoomPoint { get; }//minX, minY point 
        public Point SecondRoomPoint { get; }//maxX, maxY point
        protected Random Random;
        protected int[,] Dungeon;
        public int[,] RoomMatrix;
        protected List<Room> AllRooms;
        public Room(Point? entranceDoor, Point firstRoomPoint, Point secondRoomPoint, int[,] dungeon, List<Room> allRooms, Random random)
        {
            
            if (entranceDoor != null)
            {
                FreeDirections.Remove((Point)entranceDoor);
            }
            FirstRoomPoint = firstRoomPoint;
            SecondRoomPoint = secondRoomPoint;
            Dungeon = dungeon;
            AllRooms = allRooms;
            Random = random;
            thisDungeonSize += (SecondRoomPoint.X - FirstRoomPoint.X) * (SecondRoomPoint.Y - FirstRoomPoint.Y);
            DrawRoom();
            RoomMatrix = new int[SecondRoomPoint.Y - FirstRoomPoint.Y+1 , SecondRoomPoint.X - FirstRoomPoint.X +1];
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    RoomMatrix[i- FirstRoomPoint.Y, j- FirstRoomPoint.X] = Dungeon[i, j];
                }
            }
        }
        public virtual void DrawRoom()
        {
            for (int i = FirstRoomPoint.Y; i <= SecondRoomPoint.Y; i++)
            {
                for (int j = FirstRoomPoint.X; j <= SecondRoomPoint.X; j++)
                {
                    Dungeon[i, j] = 1;
                    Dungeon[FirstRoomPoint.Y, j] = 2;
                    Dungeon[SecondRoomPoint.Y, j] = 2;
                }
                Dungeon[i, FirstRoomPoint.X] = 2;
                Dungeon[i, SecondRoomPoint.X] = 2;
            }
        }
        public virtual void AddRoom(Point? thisDirection = null, Point? thisDoorPos = null)
        {
            if (FreeDirections.Count == 0) { return; }
            Point direction = FreeDirections[Random.Next(0, FreeDirections.Count)];
            if (thisDirection != null)
            {
                direction = (Point)thisDirection;
            }
            FreeDirections.Remove(direction);
            Point DoorPos;
            if (thisDoorPos == null)
            {
                if (direction.X == 0)
                {
                    if (direction.Y == 1)
                    {
                        DoorPos = new Point((FirstRoomPoint.X + SecondRoomPoint.X) / 2, SecondRoomPoint.Y);
                    }
                    else { DoorPos = new Point((FirstRoomPoint.X + SecondRoomPoint.X) / 2, FirstRoomPoint.Y); }
                }
                else
                {
                    if (direction.X == 1)
                    {
                        DoorPos = new Point(SecondRoomPoint.X, (FirstRoomPoint.Y + SecondRoomPoint.Y) / 2);
                    }
                    else { DoorPos = new Point(FirstRoomPoint.X, (FirstRoomPoint.Y + SecondRoomPoint.Y) / 2); }
                }
            }
            else { DoorPos = (Point)thisDoorPos; }


            Point FirstRectPoint;
            Point SecondRectPoint;
            FirstRectPoint = DoorPos + direction;
            SecondRectPoint = DoorPos + direction;
            int minX = 0;
            int minY = 0;
            int maxX = 0;
            int maxY = 0;
            if (direction.X == 0 && direction.Y != 0)
            {
                List<int> LeftLength = new List<int>();
                List<int> RightLength = new List<int>();
                int y = FirstRectPoint.Y;

                while (y >= 0 && y < DUNGEON_SIZE && Dungeon[y, FirstRectPoint.X] == 0)
                {
                    LeftLength.Add(0);
                    RightLength.Add(0);
                    int x = FirstRectPoint.X;
                    while (x >= 0 && Dungeon[y, x] == 0)
                    {
                        LeftLength[LeftLength.Count - 1]++;
                        x--;
                    }
                    x = FirstRectPoint.X;
                    while (x < DUNGEON_SIZE && Dungeon[y, x] == 0)
                    {
                        RightLength[RightLength.Count - 1]++;
                        x++;
                    }
                    y += direction.Y;
                }
                int MaxAvaliableSize = 0;
                int MinLeftLength = int.MaxValue;
                int MinRightLength = int.MaxValue;
                for (int i = 0; i < LeftLength.Count; i++)
                {
                    MinLeftLength = Math.Min(MinLeftLength, LeftLength[i] - 1);
                    MinRightLength = Math.Min(MinRightLength, RightLength[i] - 1);
                    if ((i + 1) * (MinRightLength + MinLeftLength) > MaxAvaliableSize && i >= MIN_WALL_SIZE && (MinRightLength + MinLeftLength) >= MIN_WALL_SIZE && MinLeftLength > 1 && MinRightLength > 1)
                    {
                        MaxAvaliableSize = (i + 1) * (MinRightLength + MinLeftLength);
                        FirstRectPoint = DoorPos + direction;
                        FirstRectPoint = new Point(FirstRectPoint.X - MinLeftLength, FirstRectPoint.Y);
                        SecondRectPoint = DoorPos + direction;
                        SecondRectPoint = new Point(SecondRectPoint.X + MinRightLength, FirstRectPoint.Y + (direction.Y * i));
                    }
                }
                if (MaxAvaliableSize == 0)//can'not generate room 
                {
                    Dungeon[DoorPos.Y, DoorPos.X] = 2;
                    return;
                }
                minX = Math.Min(FirstRectPoint.X, SecondRectPoint.X);
                minY = Math.Min(FirstRectPoint.Y, SecondRectPoint.Y);
                maxX = Math.Max(FirstRectPoint.X, SecondRectPoint.X);
                maxY = Math.Max(FirstRectPoint.Y, SecondRectPoint.Y);


            }
            if (direction.X != 0 && direction.Y == 0)
            {
                List<int> UpLength = new List<int>();
                List<int> DownLength = new List<int>();
                int x = FirstRectPoint.X;
                while (x > 0 && x < DUNGEON_SIZE && Dungeon[FirstRectPoint.Y, x] == 0)
                {
                    UpLength.Add(0);
                    DownLength.Add(0);
                    int y = FirstRectPoint.Y;
                    while (y >= 0 && Dungeon[y, x] == 0)
                    {
                        UpLength[UpLength.Count - 1]++;
                        y--;
                    }
                    y = FirstRectPoint.Y;
                    while (y < DUNGEON_SIZE && Dungeon[y, x] == 0)
                    {
                        DownLength[DownLength.Count - 1]++;
                        y++;
                    }
                    x += direction.X;

                }
                int MaxAvaliableSize = 0;
                int MinUpLength = int.MaxValue;
                int MinDownLength = int.MaxValue;
                for (int i = 0; i < UpLength.Count; i++)
                {
                    MinUpLength = Math.Min(MinUpLength, UpLength[i] - 1);
                    MinDownLength = Math.Min(MinDownLength, DownLength[i] - 1);
                    if ((i + 1) * (MinDownLength + MinUpLength) > MaxAvaliableSize && i >= MIN_WALL_SIZE && (MinDownLength + MinUpLength) >= MIN_WALL_SIZE && MinDownLength > 1 && MinUpLength > 1)
                    {
                        MaxAvaliableSize = (i + 1) * (MinDownLength + MinUpLength);
                        FirstRectPoint = DoorPos + direction;
                        FirstRectPoint = new Point(FirstRectPoint.X, FirstRectPoint.Y - MinUpLength);
                        SecondRectPoint = DoorPos + direction;
                        SecondRectPoint = new Point(SecondRectPoint.X + (direction.X * i), SecondRectPoint.Y + MinDownLength);
                    }
                }
                if (MaxAvaliableSize == 0)//Can'not spawn room
                {
                    Dungeon[DoorPos.Y, DoorPos.X] = 2;
                    return;
                }
                minX = Math.Min(FirstRectPoint.X, SecondRectPoint.X);
                minY = Math.Min(FirstRectPoint.Y, SecondRectPoint.Y);
                maxX = Math.Max(FirstRectPoint.X, SecondRectPoint.X);
                maxY = Math.Max(FirstRectPoint.Y, SecondRectPoint.Y);
            }

            int Height = Random.Next(MIN_WALL_SIZE, Math.Min(MAX_WALL_SIZE, maxY - minY) + 1);
            int Width = Random.Next(MIN_WALL_SIZE, Math.Min(MAX_WALL_SIZE, maxX - minX) + 1);
            bool newBossRoom = false;
            int roomShape = Random.Next(0, 5);//0-standart, 1- circle, 2-threeQuater,3-cross, 4 -cristal 5 - rombus
            if (maxX - minX >= MAX_WALL_SIZE && maxY - minY >= MAX_WALL_SIZE && !thereBossRoom && !(this is StartRoom))
            {
                Height = MAX_WALL_SIZE;
                Width = MAX_WALL_SIZE;
                thereBossRoom = true;
                newBossRoom = true;
            }
            else if (roomShape == 1)
            {
                do
                {
                    Height = Random.Next(MIN_WALL_SIZE, Math.Min(MAX_WALL_SIZE, maxY - minY) + 1);
                }
                while (Height % 2 != 0);
                do
                {
                    Width = Random.Next(MIN_WALL_SIZE, Math.Min(MAX_WALL_SIZE, maxX - minX) + 1);
                }
                while (Width % 2 != 0);
            }
            if (direction.Y == 0)
            {
                if (direction.X == 1) { maxX = minX + Width; }
                else { minX = maxX - Width; }
                minY = Math.Min(Math.Max(DoorPos.Y - Height / 2, minY), maxY - Height);
                maxY = minY + Height;
            }
            else if (direction.X == 0)
            {
                if (direction.Y == 1) { maxY = minY + Height; }
                else { minY = maxY - Height; }
                minX = Math.Min(Math.Max(DoorPos.X - Width / 2, minX), maxX - Width);
                maxX = minX + Width;
            }

            Room newRoom;
            if (!newBossRoom)
            {
                if (roomShape == 0)
                {
                    newRoom = new Room(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
                }
                else if ((Math.Abs(Math.Abs(minX - DoorPos.X) - Math.Abs(maxX - DoorPos.X)) <= 1 || Math.Abs(Math.Abs(minY - DoorPos.Y) - Math.Abs(maxY - DoorPos.Y)) <= 1) && Width % 2 == 0 && Height % 2 == 0 && roomShape == 1)
                {
                    newRoom = new CircleRoom(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
                }
                else if (roomShape == 2)
                {
                    newRoom = new ThreeQuarterRoom(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
                }
                else if (roomShape == 3 && (Math.Abs(Math.Abs(minX - DoorPos.X) - Math.Abs(maxX - DoorPos.X)) <= 1 || Math.Abs(Math.Abs(minY - DoorPos.Y) - Math.Abs(maxY - DoorPos.Y)) <= 1))
                {
                    newRoom = new CrossRoom(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
                }
                else if (roomShape == 4 && (Math.Abs(Math.Abs(minX - DoorPos.X) - Math.Abs(maxX - DoorPos.X)) <= 1 || Math.Abs(Math.Abs(minY - DoorPos.Y) - Math.Abs(maxY - DoorPos.Y)) <= 1))
                {
                    newRoom = new CristalRoom(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
                }
                else
                {
                    newRoom = new Room(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
                }
            }
            else
            {
                newRoom = new BossRoom(new Point(direction.X * -1, direction.Y * -1), new Point(minX, minY), new Point(maxX, maxY), Dungeon, AllRooms, Random);
            }
            AllRooms.Add(newRoom);
            Dungeon[DoorPos.Y + direction.Y, DoorPos.X + direction.X] = 3;
            Dungeon[DoorPos.Y, DoorPos.X] = 3;
            RoomMatrix[DoorPos.Y - FirstRoomPoint.Y, DoorPos.X -FirstRoomPoint.X] = 3;////////////////
            newRoom.RoomMatrix[DoorPos.Y + direction.Y - newRoom.FirstRoomPoint.Y, DoorPos.X + direction.X - newRoom.FirstRoomPoint.X] = 3;
        }

    }
    public (int[,],List<Room> allRooms) Generate()
    {
        int[,] Dungeon = new int[DUNGEON_SIZE, DUNGEON_SIZE];
        Random random = new Random();
        StartRoom startRoom = new StartRoom(null, new Point(48, 48), new Point(52, 52), Dungeon, AllRooms, random);
        AllRooms.Add(startRoom);
        thereBossRoom = true;
        startRoom.AddRoom();
        //int RoomQuantity = random.Next(MinRoomsQuantity, MaxRoomsQuantity);
        int TargetDungeonSize = random.Next(MIN_DUNGEON_SIZE, MAX_DUNGEON_SIZE);
        while (thisDungeonSize < TargetDungeonSize)
        {
            Room thisRoom = AllRooms[random.Next(0, AllRooms.Count)];
            thisRoom.AddRoom();
        }
        thereBossRoom = false;
        do
        {
            Room thisRoom = AllRooms[random.Next(1, AllRooms.Count)];
            thisRoom.AddRoom();
        } while (!thereBossRoom);
        return (Dungeon,AllRooms);
    }
}

