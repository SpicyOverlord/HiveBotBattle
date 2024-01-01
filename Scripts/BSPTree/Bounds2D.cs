using System;
using System.Numerics;
using Godot;
using Utils;
using Vector2 = Godot.Vector2;

namespace HiveBotBattle.Scripts.BSPTree
{
    public readonly struct Bounds2D
    {
        public readonly int xMin;
        public readonly int xMax;
        public readonly int yMin;
        public readonly int yMax;

        public Bounds2D(float xMin, float xMax, float yMin, float yMax)
        {
            this.xMin = (int)xMin;
            this.xMax = (int)xMax;
            this.yMin = (int)yMin;
            this.yMax = (int)yMax;
        }

        public Bounds2D(Vector2 min, Vector2 max)
        {
            xMin = (int)min.X;
            xMax = (int)max.X;
            yMin = (int)min.Y;
            yMax = (int)max.Y;
        }

        public override string ToString() => $"[({xMin},{yMin})({xMax},{yMax})]";

        public bool IsPosInside(Vector2 pos) => xMin <= pos.X && pos.X <= xMax &&
                   yMin <= pos.Y && pos.Y <= yMax;

        public bool IsPosInside(Pos pos) => xMin <= pos.X && pos.X <= xMax &&
                   yMin <= pos.Y && pos.Y <= yMax;

        public bool BoundsOverlaps(Bounds2D otherBounds)
        {
            if (otherBounds.xMax < xMin || xMax < otherBounds.xMin || 
                otherBounds.yMax < yMin || yMax < otherBounds.yMin)
                return false;
            return true;
        }
    }
}