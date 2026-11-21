using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Skill Nova Bomb (3x3 Crater):
/// Khi khối gạch khóa xuống bàn cờ, phát nổ toàn bộ các ô gạch trong vùng vuông bán kính N (mặc định 3x3)
/// xung quanh tâm của khối gạch, giúp cứu nguy khi đặt nhầm (misdrop) tạo lỗ hổng chết.
/// Tối ưu Zero-GC với buffer tái sử dụng và WaitForSeconds cache sẵn.
/// </summary>
public class NovaBombSkill : ISkillEffect
{
    private readonly int _radius;
    private readonly float _flashDuration;
    private readonly int _flashCount;
    private readonly WaitForSeconds _cachedFlashWait;

    private readonly List<Vector3Int> _blastCellsBuffer = new List<Vector3Int>(25);
    private readonly List<Vector3Int> _flashPositionsBuffer = new List<Vector3Int>(25);
    private readonly List<TileBase> _flashTilesBuffer = new List<TileBase>(25);

    public NovaBombSkill(int radius = 1, float flashDuration = 0.24f, int flashCount = 2)
    {
        _radius = Mathf.Max(1, radius);
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

        Vector3Int center = CalculatePieceCenter(ghostCells, ghostPosition);
        RectInt bounds = board.Bounds;

        for (int dy = -_radius; dy <= _radius; dy++)
        {
            for (int dx = -_radius; dx <= _radius; dx++)
            {
                Vector3Int cell = new Vector3Int(center.x + dx, center.y + dy, 0);
                if (bounds.Contains((Vector2Int)cell))
                {
                    previewCellsBuffer.Add(cell);
                }
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

        GetPreviewCells(board, lockedPiece.Cells, lockedPiece.Position, _blastCellsBuffer);

        Tilemap tilemap = board.Tilemap;
        _flashPositionsBuffer.Clear();
        _flashTilesBuffer.Clear();

        int blastCount = _blastCellsBuffer.Count;
        for (int i = 0; i < blastCount; i++)
        {
            Vector3Int pos = _blastCellsBuffer[i];
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                _flashPositionsBuffer.Add(pos);
                _flashTilesBuffer.Add(tile);
            }
        }

        // Hiệu ứng nhấp nháy vùng nổ 3x3 (Zero-GC)
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

        // Phá hủy toàn bộ gạch trong vùng nổ 3x3
        for (int i = 0; i < blastCount; i++)
        {
            tilemap.SetTile(_blastCellsBuffer[i], null);
        }

        // Ghi nhận hàng tâm để hiển thị hiệu ứng nếu cần, Nova Bomb không tự tính là xóa hàng ngang
        if (affectedRowsBuffer != null && blastCount > 0)
        {
            Vector3Int center = CalculatePieceCenter(lockedPiece.Cells, lockedPiece.Position);
            affectedRowsBuffer.Add(center.y);
        }

        onLinesClearedCallback?.Invoke(0);
    }

    private static Vector3Int CalculatePieceCenter(Vector3Int[] cells, Vector3Int origin)
    {
        int sumX = 0;
        int sumY = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            sumX += cells[i].x + origin.x;
            sumY += cells[i].y + origin.y;
        }

        return new Vector3Int(
            Mathf.RoundToInt((float)sumX / cells.Length),
            Mathf.RoundToInt((float)sumY / cells.Length),
            0);
    }
}
