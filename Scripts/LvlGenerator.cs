using System.Diagnostics;
using HiveBotBattle.Scripts.Utils.Types;

namespace HiveBotBattle.Scripts
{
    public class LvlGenerator
    {
        public static CellType[][] EvenLevel(int width, int height)
        {
            CellType[][] lvl = new CellType[width][];

            for (int x = 0; x < width; x++)
            {
                lvl[x] = new CellType[height];

                for (int y = 0; y < height; y++)
                {
                    // bedrock edges
                    if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                    {
                        lvl[x][y] = CellType.Bedrock;
                        continue;
                    }
                    if (y == 1 || y == height - 2 || x == 1 || x == width - 2)
                    {
                        lvl[x][y] = CellType.Stone;
                        continue;
                    }

                    // content
                    if (x % 3 == 0)
                        lvl[x][y] = CellType.Stone;
                    else if (y % 3 == 0 || x % 4 == y % 4)
                        lvl[x][y] = CellType.Deposit;

                    // randomize map with stone and deposits
                    // if (Random.Range(0, 8) == 0) CreateVisual(x, y, CellType.Stone);
                    // else if (Random.Range(0, 4) == 0) CreateVisual(x, y, CellType.Deposit);
                }
            }

            return lvl;
        }

        public static CellType DiagonalLines(int x, int y)
        {
            if (x % 3 == 0)
                return CellType.Stone;
            if (y % 3 == 0 || x % 4 == y % 4)
                return CellType.Deposit;

            return CellType.Empty;
        }

        public static CellType OnlyDeposit(int x, int y)
        {
            return CellType.Deposit;
        }
    }
}