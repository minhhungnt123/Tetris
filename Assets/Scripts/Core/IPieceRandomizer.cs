using System.Collections.Generic;

/// <summary>
/// Interface trừu tượng cho thuật toán sinh khối ngẫu nhiên (DIP & OCP).
/// Cho phép hoán đổi giữa 7-Bag (Guideline), 14-Bag, NES RNG hoặc True Random.
/// </summary>
public interface IPieceRandomizer
{
    void Initialize(IReadOnlyList<TetrominoData> availablePieces);
    TetrominoData GetNextPiece();
    TetrominoData PeekNextPiece();

    /// <summary>
    /// Xem trước <paramref name="count"/> khối tiếp theo và ghi vào <paramref name="resultsBuffer"/>
    /// do caller cung cấp nhằm tránh cấp phát bộ nhớ (Zero-GC).
    /// </summary>
    void PeekNextPieces(int count, List<TetrominoData> resultsBuffer);

    void Reset();
}
