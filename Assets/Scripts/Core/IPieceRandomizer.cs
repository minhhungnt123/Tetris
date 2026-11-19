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
    void Reset();
}
