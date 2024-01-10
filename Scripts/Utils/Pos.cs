using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using HiveBotBattle.Scripts;

namespace Utils
{
    /// <summary>
    /// Represents a position in a 2D grid.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class Pos
    {
        /// <summary>
        /// The spacing between positions in the grid.
        /// </summary>
        public const int PositionSpacing = 64;

        /// <summary>
        /// The X coordinate of the position.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y coordinate of the position.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pos"/> class with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Adds the specified position to this position and returns a new position.
        /// </summary>
        /// <param name="pos">The position to add.</param>
        /// <returns>A new position that is the result of adding the specified position to this position.</returns>
        public Pos AddPosition(Pos pos) => AddPosition(pos.X, pos.Y);

        /// <summary>
        /// Adds the specified coordinates to this position and returns a new position.
        /// </summary>
        /// <param name="x">The X coordinate to add.</param>
        /// <param name="y">The Y coordinate to add.</param>
        /// <returns>A new position that is the result of adding the specified coordinates to this position.</returns>
        public Pos AddPosition(int x, int y) => new Pos(X + x, Y + y);

        /// <summary>
        /// Returns a string representation of the position.
        /// </summary>
        /// <returns>A string representation of the position.</returns>
        public override string ToString() => $"({X},{Y})";

        /// <summary>
        /// Returns a hash code for the position.
        /// </summary>
        /// <returns>A hash code for the position.</returns>
        public override int GetHashCode() => X * 10000 + Y;

        /// <summary>
        /// Determines whether this position is equal to the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate to compare.</param>
        /// <param name="y">The Y coordinate to compare.</param>
        /// <returns><c>true</c> if this position is equal to the specified coordinates; otherwise, <c>false</c>.</returns>
        public bool Equals(int x, int y) => X == x && Y == y;

        public static bool operator ==(Pos left, Pos right)
        {
            if (left is null || right is null)
                return false;

            return left.X == right.X && left.Y == right.Y;
        }
        public static bool operator !=(Pos left, Pos right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Creates a new position that is a copy of this position.
        /// </summary>
        /// <returns>A new position that is a copy of this position.</returns>
        public Pos Clone() => new Pos(X, Y);

        /// <summary>
        /// Returns the position as a <see cref="Vector2"/> with the coordinates scaled by the position spacing.
        /// </summary>
        /// <returns>The position as a <see cref="Vector2"/>.</returns>
        public Vector2 GetAsVector2() => new Vector2(X * PositionSpacing, Y * PositionSpacing);

        /// <summary>
        /// Determines whether this position is next to or equal to the specified position.
        /// </summary>
        /// <param name="other">The position to compare.</param>
        /// <returns><c>true</c> if this position is next to or equal to the specified position; otherwise, <c>false</c>.</returns>
        public bool IsNextToOrEqual(Pos other) => IsNextToOrEqual(other.X, other.Y);

        /// <summary>
        /// Determines whether this position is next to or equal to the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate to compare.</param>
        /// <param name="y">The Y coordinate to compare.</param>
        /// <returns><c>true</c> if this position is next to or equal to the specified coordinates; otherwise, <c>false</c>.</returns>
        public bool IsNextToOrEqual(int x, int y)
        {
            int xDiff = X - x;
            int yDiff = Y - y;
            return (xDiff == 0 || xDiff == 1 || xDiff == -1) && (yDiff == 0 || yDiff == 1 || yDiff == -1);
        }

        /// <summary>
        /// Calculates the distance between this position and the specified position.
        /// </summary>
        /// <param name="otherPos">The position to calculate the distance to.</param>
        /// <returns>The distance between this position and the specified position.</returns>
        public float DistanceTo(Pos otherPos) => MathF.Sqrt(DistanceToSquared(otherPos));

        /// <summary>
        /// Calculates the squared distance between this position and the specified position.
        /// </summary>
        /// <param name="otherPos">The position to calculate the squared distance to.</param>
        /// <returns>The squared distance between this position and the specified position.</returns>
        public int DistanceToSquared(Pos otherPos)
        {
            int xDiff = X - otherPos.X;
            int yDiff = Y - otherPos.Y;
            return xDiff * xDiff + yDiff * yDiff;
        }

        /// <summary>
        /// Calculates the distance between two positions.
        /// </summary>
        /// <param name="x1">The X coordinate of the first position.</param>
        /// <param name="y1">The Y coordinate of the first position.</param>
        /// <param name="x2">The X coordinate of the second position.</param>
        /// <param name="y2">The Y coordinate of the second position.</param>
        /// <returns>The distance between the two positions.</returns>
        public static float DistanceTo(int x1, int y1, int x2, int y2) => MathF.Sqrt(SquaredDistanceTo(x1, y1, x2, y2));

        /// <summary>
        /// Calculates the squared distance between two positions.
        /// </summary>
        /// <param name="x1">The X coordinate of the first position.</param>
        /// <param name="y1">The Y coordinate of the first position.</param>
        /// <param name="x2">The X coordinate of the second position.</param>
        /// <param name="y2">The Y coordinate of the second position.</param>
        /// <returns>The squared distance between the two positions.</returns>
        public static int SquaredDistanceTo(int x1, int y1, int x2, int y2)
        {
            int xDiff = x1 - x2;
            int yDiff = y1 - y2;
            return xDiff * xDiff + yDiff * yDiff;
        }

        /// <summary>
        /// Determines whether this position is within the shooting range of the specified position.
        /// </summary>
        /// <param name="otherPos">The position to check.</param>
        /// <returns><c>true</c> if this position is within the shooting range of the specified position; otherwise, <c>false</c>.</returns>
        public bool InShootingRangeOf(Pos otherPos) => DistanceTo(otherPos) <= Bot.ShootingRange;

        /// <summary>
        /// Gets the neighboring positions of this position.
        /// </summary>
        /// <returns>A list of neighboring positions.</returns>
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

        /// <summary>
        /// Gets the position above this position.
        /// </summary>
        /// <returns>The position above this position.</returns>
        public Pos Up() => new Pos(X, Y + 1);

        /// <summary>
        /// Gets the position below this position.
        /// </summary>
        /// <returns>The position below this position.</returns>
        public Pos Down() => new Pos(X, Y - 1);

        /// <summary>
        /// Gets the position to the left of this position.
        /// </summary>
        /// <returns>The position to the left of this position.</returns>
        public Pos Left() => new Pos(X - 1, Y);

        /// <summary>
        /// Gets the position to the right of this position.
        /// </summary>
        /// <returns>The position to the right of this position.</returns>
        public Pos Right() => new Pos(X + 1, Y);

        /// <summary>
        /// Gets the position above and to the left of this position.
        /// </summary>
        /// <returns>The position above and to the left of this position.</returns>
        public Pos UpLeft() => new Pos(X - 1, Y + 1);

        /// <summary>
        /// Gets the position above and to the right of this position.
        /// </summary>
        /// <returns>The position above and to the right of this position.</returns>
        public Pos UpRight() => new Pos(X + 1, Y + 1);

        /// <summary>
        /// Gets the position below and to the left of this position.
        /// </summary>
        /// <returns>The position below and to the left of this position.</returns>
        public Pos DownLeft() => new Pos(X - 1, Y - 1);

        /// <summary>
        /// Gets the position below and to the right of this position.
        /// </summary>
        /// <returns>The position below and to the right of this position.</returns>
        public Pos DownRight() => new Pos(X + 1, Y - 1);

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}