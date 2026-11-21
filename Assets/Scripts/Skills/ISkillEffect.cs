using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface chiến lược cho mọi hiệu ứng kỹ năng đặc biệt (Strategy Pattern - OCP & DIP).
/// Thiết kế truyền buffer từ caller để đảm bảo 0 bytes GC khi vẽ Blast Zone Preview và khi thực thi nổ.
/// </summary>
public interface ISkillEffect
{
    /// <summary>
    /// Tính toán danh sách các ô sẽ bị ảnh hưởng (Blast Zone Preview) dựa trên vị trí Ghost hiện tại.
    /// Ghi kết quả vào <paramref name="previewCellsBuffer"/> (Zero-GC).
    /// </summary>
    void GetPreviewCells(
        Board board,
        Vector3Int[] ghostCells,
        Vector3Int ghostPosition,
        List<Vector3Int> previewCellsBuffer);

    /// <summary>
    /// Thực thi hiệu ứng khi khối gạch đặc biệt khóa xuống bàn cờ (Coroutine tối ưu GC).
    /// Ghi các hàng bị ảnh hưởng vào <paramref name="affectedRowsBuffer"/> và báo tổng số dòng xóa qua callback.
    /// </summary>
    IEnumerator ExecuteOnLock(
        Board board,
        Piece lockedPiece,
        List<int> affectedRowsBuffer,
        Action<int> onLinesClearedCallback);
}
