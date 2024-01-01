using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Godot;
using HiveBotBattle.Scripts;


namespace Utils
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class Pos
    {
        public const int PositionSpacing = 64;
        public int X { get; private set; }
        public int Y { get; private set; }

        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Pos AddPosition(Pos pos) => AddPosition(pos.X, pos.Y);
        public Pos AddPosition(int x, int y) => new Pos(X + x, Y + y);

        public override string ToString() => $"({X},{Y})";
        public override int GetHashCode() => X * 10000 + Y;

        public bool Equals(int x, int y) => X == x && Y == y;

        public Pos Clone() => new Pos(X, Y);

        public Vector2 GetAsVector2() => new Vector2(X * PositionSpacing, Y * PositionSpacing);

        public bool IsNextToOrEqual(Pos other) => IsNextToOrEqual(other.X, other.Y);
        public bool IsNextToOrEqual(int x, int y)
        {
            int xDiff = X - x;
            int yDiff = Y - y;
            return (xDiff == 0 || xDiff == 1 || xDiff == -1) && (yDiff == 0 || yDiff == 1 || yDiff == -1);
        }

        public float DistanceTo(Pos otherPos) => MathF.Sqrt(DistanceToSquared(otherPos));
        public int DistanceToSquared(Pos otherPos)
        {
            int xDiff = X - otherPos.X;
            int yDiff = Y - otherPos.Y;
            return xDiff * xDiff + yDiff * yDiff;
        }

        public static float DistanceTo(int x1, int y1, int x2, int y2) => MathF.Sqrt(SquaredDistanceTo(x1, y1, x2, y2));
        public static int SquaredDistanceTo(int x1, int y1, int x2, int y2)
        {
            int xDiff = x1 - x2;
            int yDiff = y1 - y2;
            return xDiff * xDiff + yDiff * yDiff;
        }

        public bool InShootingRangeOf(Pos otherPos) => DistanceTo(otherPos) <= Bot.ShootingRange;


        public List<Pos> GetNeighbors()
        {
            List<Pos> neighbors = new List<Pos>();

            for (int yDiff = 1; yDiff >= -1; yDiff--)
                for (int xDiff = -1; xDiff <= 1; xDiff++)
                {
                    if (xDiff == 0 && yDiff == 0)
                        continue;
                    neighbors.Add(AddPosition(xDiff, yDiff));
                }

            return neighbors;
        }

        public Pos Up() => new Pos(X, Y + 1);
        public Pos Down() => new Pos(X, Y - 1);
        public Pos Left() => new Pos(X - 1, Y);
        public Pos Right() => new Pos(X + 1, Y);
        public Pos UpLeft() => new Pos(X - 1, Y + 1);
        public Pos UpRight() => new Pos(X + 1, Y + 1);
        public Pos DownLeft() => new Pos(X - 1, Y - 1);
        public Pos DownRight() => new Pos(X + 1, Y - 1);

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}