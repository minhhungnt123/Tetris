using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thuật toán 7-Bag Randomizer theo chuẩn Tetris Guideline:
/// Mỗi túi (bag) chứa đúng 7 loại khối ngẫu nhiên được xáo trộn bằng Fisher-Yates shuffle.
/// Đảm bảo không bao giờ bị "hạn hán" (drought) một loại khối quá 12 lượt.
/// </summary>
public class SevenBagRandomizer : IPieceRandomizer
{
    private readonly List<TetrominoData> _availablePieces = new List<TetrominoData>();
    private readonly Queue<TetrominoData> _pieceQueue = new Queue<TetrominoData>();
    private readonly List<TetrominoData> _shuffleBuffer = new List<TetrominoData>();

    public void Initialize(IReadOnlyList<TetrominoData> availablePieces)
    {
        _availablePieces.Clear();
        if (availablePieces != null)
        {
            _availablePieces.AddRange(availablePieces);
        }
        Reset();
    }

    public void Reset()
    {
        _pieceQueue.Clear();
        RefillBag();
    }

    public TetrominoData GetNextPiece()
    {
        if (_pieceQueue.Count == 0)
        {
            RefillBag();
        }
        return _pieceQueue.Dequeue();
    }

    public TetrominoData PeekNextPiece()
    {
        if (_pieceQueue.Count == 0)
        {
            RefillBag();
        }
        return _pieceQueue.Peek();
    }

    private void RefillBag()
    {
        if (_availablePieces.Count == 0)
        {
            Debug.LogWarning("[SevenBagRandomizer] Không có dữ liệu Tetromino để sinh khối!");
            return;
        }

        _shuffleBuffer.Clear();
        _shuffleBuffer.AddRange(_availablePieces);

        // Thuật toán Fisher-Yates shuffle phân phối đều
        for (int i = _shuffleBuffer.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_shuffleBuffer[i], _shuffleBuffer[j]) = (_shuffleBuffer[j], _shuffleBuffer[i]);
        }

        for (int i = 0; i < _shuffleBuffer.Count; i++)
        {
            _pieceQueue.Enqueue(_shuffleBuffer[i]);
        }
    }
}
