using System;
using System.Numerics;
using Godot;
using Utils;
using Vector2 = Godot.Vector2;

namespace HiveBotBattle.Scripts.BSPTree
{
    /// <summary>
    /// Represents a 2D bounding box with integer coordinates.
    /// Used to represent the bounds of a BSP tree partition.
    /// </summary>
    public readonly struct Bounds2D
    {
        /// <summary>
        /// The minimum x-coordinate of the bounding box.
        /// </summary>
        public readonly int xMin;

        /// <summary>
        /// The maximum x-coordinate of the bounding box.
        /// </summary>
        public readonly int xMax;

        /// <summary>
        /// The minimum y-coordinate of the bounding box.
        /// </summary>
        public readonly int yMin;

        /// <summary>
        /// The maximum y-coordinate of the bounding box.
        /// </summary>
        public readonly int yMax;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bounds2D"/> struct.
        /// </summary>
        /// <param name="xMin">The minimum x-coordinate of the bounding box.</param>
        /// <param name="xMax">The maximum x-coordinate of the bounding box.</param>
        /// <param name="yMin">The minimum y-coordinate of the bounding box.</param>
        /// <param name="yMax">The maximum y-coordinate of the bounding box.</param>
        public Bounds2D(float xMin, float xMax, float yMin, float yMax)
        {
            this.xMin = (int)xMin;
            this.xMax = (int)xMax;
            this.yMin = (int)yMin;
            this.yMax = (int)yMax;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bounds2D"/> struct.
        /// </summary>
        /// <param name="min">The minimum coordinates of the bounding box.</param>
        /// <param name="max">The maximum coordinates of the bounding box.</param>
        public Bounds2D(Vector2 min, Vector2 max)
        {
            xMin = (int)min.X;
            xMax = (int)max.X;
            yMin = (int)min.Y;
            yMax = (int)max.Y;
        }

        /// <summary>
        /// Returns a string representation of the bounding box.
        /// </summary>
        /// <returns>A string representation of the bounding box.</returns>
        public override string ToString() => $"[({xMin},{yMin})({xMax},{yMax})]";

        /// <summary>
        /// Checks if a position is inside the bounding box.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True if the position is inside the bounding box, false otherwise.</returns>
        public bool IsPosInside(Vector2 pos) => xMin <= pos.X && pos.X <= xMax &&
                   yMin <= pos.Y && pos.Y <= yMax;

        /// <summary>
        /// Checks if a position is inside the bounding box.
        /// </summary>
        /// <param name="pos">The position to check.</param>
        /// <returns>True if the position is inside the bounding box, false otherwise.</returns>
        public bool IsPosInside(Pos pos) => xMin <= pos.X && pos.X <= xMax &&
                   yMin <= pos.Y && pos.Y <= yMax;

        /// <summary>
        /// Checks if the bounding box overlaps with another bounding box.
        /// </summary>
        /// <param name="otherBounds">The other bounding box to check.</param>
        /// <returns>True if the bounding boxes overlap, false otherwise.</returns>
        public bool BoundsOverlaps(Bounds2D otherBounds)
        {
            if (otherBounds.xMax < xMin || xMax < otherBounds.xMin || 
                otherBounds.yMax < yMin || yMax < otherBounds.yMin)
                return false;
            return true;
        }
    }
}