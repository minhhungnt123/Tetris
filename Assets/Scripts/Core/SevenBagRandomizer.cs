using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thuật toán 7-Bag Randomizer theo chuẩn Tetris Guideline:
/// Mỗi túi (bag) chứa đúng 7 loại khối ngẫu nhiên được xáo trộn bằng Fisher-Yates shuffle.
/// Sử dụng List kèm chỉ số đầu (_headIndex) thay cho Queue để hỗ trợ Peek nhiều khối (4 Next)
/// mà không phát sinh cấp phát bộ nhớ (Zero-GC).
/// </summary>
public class SevenBagRandomizer : IPieceRandomizer
{
    private readonly List<TetrominoData> _availablePieces = new List<TetrominoData>(7);
    private readonly List<TetrominoData> _pieceQueue = new List<TetrominoData>(21);
    private readonly List<TetrominoData> _shuffleBuffer = new List<TetrominoData>(7);
    private int _headIndex;

    private int RemainingCount => _pieceQueue.Count - _headIndex;

    public void Initialize(IReadOnlyList<TetrominoData> availablePieces)
    {
        _availablePieces.Clear();
        if (availablePieces != null)
        {
            for (int i = 0; i < availablePieces.Count; i++)
            {
                _availablePieces.Add(availablePieces[i]);
            }
        }
        Reset();
    }

    public void Reset()
    {
        _pieceQueue.Clear();
        _headIndex = 0;
        RefillBag();
    }

    public TetrominoData GetNextPiece()
    {
        EnsureQueueHasAtLeast(1);
        if (RemainingCount <= 0)
        {
            return default;
        }

        TetrominoData next = _pieceQueue[_headIndex];
        _headIndex++;

        // Thu gọn bộ đệm khi đã tiêu thụ ít nhất 1 túi đầy để tránh phình List và không tạo mảng mới
        if (_headIndex >= _availablePieces.Count && _availablePieces.Count > 0)
        {
            _pieceQueue.RemoveRange(0, _headIndex);
            _headIndex = 0;
        }

        return next;
    }

    public TetrominoData PeekNextPiece()
    {
        EnsureQueueHasAtLeast(1);
        return RemainingCount > 0 ? _pieceQueue[_headIndex] : default;
    }

    public void PeekNextPieces(int count, List<TetrominoData> resultsBuffer)
    {
        if (resultsBuffer == null) return;

        resultsBuffer.Clear();
        if (count <= 0) return;

        EnsureQueueHasAtLeast(count);

        int available = RemainingCount;
        int copyCount = count < available ? count : available;
        for (int i = 0; i < copyCount; i++)
        {
            resultsBuffer.Add(_pieceQueue[_headIndex + i]);
        }
    }

    private void EnsureQueueHasAtLeast(int requiredCount)
    {
        if (_availablePieces.Count == 0) return;

        while (RemainingCount < requiredCount)
        {
            RefillBag();
        }
    }

    private void RefillBag()
    {
        if (_availablePieces.Count == 0)
        {
            Debug.LogWarning("[SevenBagRandomizer] Không có dữ liệu Tetromino để sinh khối!");
            return;
        }

        _shuffleBuffer.Clear();
        for (int i = 0; i < _availablePieces.Count; i++)
        {
            _shuffleBuffer.Add(_availablePieces[i]);
        }

        // Thuật toán Fisher-Yates shuffle phân phối đều
        for (int i = _shuffleBuffer.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_shuffleBuffer[i], _shuffleBuffer[j]) = (_shuffleBuffer[j], _shuffleBuffer[i]);
        }

        for (int i = 0; i < _shuffleBuffer.Count; i++)
        {
            _pieceQueue.Add(_shuffleBuffer[i]);
        }
    }
}
