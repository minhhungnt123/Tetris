using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Skill Row Detonator (Plasma Cleave):
/// Khi khối gạch khóa xuống bàn cờ, phát nổ và xóa sạch toàn bộ các hàng ngang mà khối này chiếm giữ,
/// bất kể hàng đó có lỗ hổng hay chưa lấp đủ 10 ô, sau đó dồn các hàng phía trên rơi xuống.
/// Tối ưu Zero-GC với buffer tái sử dụng và WaitForSeconds cache sẵn.
/// </summary>
public class RowDetonatorSkill : ISkillEffect
{
    private readonly float _flashDuration;
    private readonly int _flashCount;
    private readonly WaitForSeconds _cachedFlashWait;

    private readonly List<int> _uniqueRowsBuffer = new List<int>(4);
    private readonly List<Vector3Int> _flashPositionsBuffer = new List<Vector3Int>(40);
    private readonly List<TileBase> _flashTilesBuffer = new List<TileBase>(40);

    public RowDetonatorSkill(float flashDuration = 0.24f, int flashCount = 2)
    {
        _flashDuration = Mathf.Max(0.05f, flashDuration);
        _flashCount = Mathf.Max(1, flashCount);
        _cachedFlashWait = new WaitForSeconds(_flashDuration / (_flashCount * 2f));
    }

    public void GetPreviewCells(
        Board board,
        Vector3Int[] ghostCells,
        Vector3Int ghostPosition,
        List<Vector3Int> previewCellsBuffer)
    {
        if (previewCellsBuffer == null) return;
        previewCellsBuffer.Clear();

        if (board == null || ghostCells == null || ghostCells.Length == 0) return;

        RectInt bounds = board.Bounds;
        CollectUniqueRows(ghostCells, ghostPosition, bounds, _uniqueRowsBuffer);

        int rowCount = _uniqueRowsBuffer.Count;
        for (int r = 0; r < rowCount; r++)
        {
            int row = _uniqueRowsBuffer[r];
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                previewCellsBuffer.Add(new Vector3Int(col, row, 0));
            }
        }
    }

    public IEnumerator ExecuteOnLock(
        Board board,
        Piece lockedPiece,
        List<int> affectedRowsBuffer,
        Action<int> onLinesClearedCallback)
    {
        if (affectedRowsBuffer != null) affectedRowsBuffer.Clear();

        if (board == null || board.Tilemap == null || lockedPiece == null || lockedPiece.Cells == null)
        {
            onLinesClearedCallback?.Invoke(0);
            yield break;
        }

        RectInt bounds = board.Bounds;
        CollectUniqueRows(lockedPiece.Cells, lockedPiece.Position, bounds, _uniqueRowsBuffer);

        if (_uniqueRowsBuffer.Count == 0)
        {
            onLinesClearedCallback?.Invoke(0);
            yield break;
        }

        // Lưu danh sách các hàng bị nổ vào affectedRowsBuffer
        if (affectedRowsBuffer != null)
        {
            for (int i = 0; i < _uniqueRowsBuffer.Count; i++)
            {
                affectedRowsBuffer.Add(_uniqueRowsBuffer[i]);
            }
        }

        Tilemap tilemap = board.Tilemap;
        _flashPositionsBuffer.Clear();
        _flashTilesBuffer.Clear();

        int rowCount = _uniqueRowsBuffer.Count;
        for (int r = 0; r < rowCount; r++)
        {
            int row = _uniqueRowsBuffer[r];
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int pos = new Vector3Int(col, row, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {
                    _flashPositionsBuffer.Add(pos);
                    _flashTilesBuffer.Add(tile);
                }
            }
        }

        // Chạy hiệu ứng chớp sáng vùng nổ (Zero-GC)
        int tileCount = _flashPositionsBuffer.Count;
        for (int blink = 0; blink < _flashCount; blink++)
        {
            for (int i = 0; i < tileCount; i++)
            {
                tilemap.SetTile(_flashPositionsBuffer[i], null);
            }
            yield return _cachedFlashWait;

            for (int i = 0; i < tileCount; i++)
            {
                tilemap.SetTile(_flashPositionsBuffer[i], _flashTilesBuffer[i]);
            }
            yield return _cachedFlashWait;
        }

        // Xóa sạch toàn bộ các hàng mục tiêu và dồn gạch phía trên rơi xuống
        board.CollapseRows(_uniqueRowsBuffer);

        onLinesClearedCallback?.Invoke(_uniqueRowsBuffer.Count);
    }

    private static void CollectUniqueRows(
        Vector3Int[] cells,
        Vector3Int origin,
        RectInt bounds,
        List<int> outputRows)
    {
        outputRows.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            int row = cells[i].y + origin.y;
            if (row < bounds.yMin || row >= bounds.yMax) continue;

            bool exists = false;
            for (int j = 0; j < outputRows.Count; j++)
            {
                if (outputRows[j] == row)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                outputRows.Add(row);
            }
        }
    }
}
