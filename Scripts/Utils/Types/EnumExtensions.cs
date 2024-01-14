using System;
using HiveBotBattle.Scripts.Utils.Types;

public static class TypeExtensions
{
    public static string ToString(this CellType cellType)
    {
        return cellType switch
        {
            CellType.None => "None",
            CellType.Empty => "Empty",
            CellType.Stone => "Stone",
            CellType.Deposit => "Deposit",
            CellType.Mineral => "Mineral",
            CellType.Bedrock => "Bedrock",
            CellType.FighterBot => "FighterBot",
            CellType.MinerBot => "MinerBot",
            CellType.MotherShip => "MotherShip",
            _ => throw new Exception("Value '" + cellType + "' not recognized!")
        };
    }
}