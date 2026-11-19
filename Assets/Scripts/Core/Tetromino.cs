using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;

    public Vector2Int[] cells => Data.Cells.TryGetValue(tetromino, out var c) ? c : null;
    public Vector2Int[,] wallKicks => Data.WallKicks.TryGetValue(tetromino, out var wk) ? wk : null;

    public void Initialize()
    {
        // Kept for backward compatibility
    }
}