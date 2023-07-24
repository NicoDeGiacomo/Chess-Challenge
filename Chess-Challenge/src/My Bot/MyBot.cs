using System;
using ChessChallenge.API;
using ChessChallenge.Application;

public class MyBot : IChessBot
{
    private const int MaxDepth = 4;
    private readonly int[] _pieceWeights = { 0, 100, 320, 330, 500, 900, 20000 };
    private Board _board;

    private readonly int[] _pawnTable =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        5, 10, 10, -20, -20, 10, 10, 5,
        5, -5, -10, 0, 0, -10, -5, 5,
        0, 0, 0, 20, 20, 0, 0, 0,
        5, 5, 10, 25, 25, 10, 5, 5,
        10, 10, 20, 30, 30, 20, 10, 10,
        50, 50, 50, 50, 50, 50, 50, 50,
        0, 0, 0, 0, 0, 0, 0, 0
    };

    public Move Think(Board board, Timer timer)
    {
        _board = board;
        Span<Move> moves = stackalloc Move[256];
        _board.GetLegalMovesNonAlloc(ref moves);
        ConsoleHelper.Log("Evaluation: " + Evaluate());
        return moves[0];
    }

    private int Evaluate()
    {
        if (_board.IsInCheckmate())
            return (_board.IsWhiteToMove ? 1 : -1) * -1000000;
        if (_board.IsDraw())
            return 0;


        int materialScore = 0;
        for (int i = 0; i < 8; i++)
        for (int j = 0; j < 8; j++)
        {
            Piece piece = _board.GetPiece(new Square(i, j));
            if (piece.PieceType != PieceType.None)
                materialScore += _pieceWeights[(int)piece.PieceType] * (piece.IsWhite ? 1 : -1);
        }

        int whitePawnScore = 0;
        foreach (Piece pawn in _board.GetPieceList(PieceType.Pawn, true))
        {
            whitePawnScore += _pawnTable[pawn.Square.Index];
        }

        int blackPawnScore = 0;
        foreach (Piece pawn in _board.GetPieceList(PieceType.Pawn, false))
        {
            blackPawnScore -= _pawnTable[63 - pawn.Square.Index];
        }

        ConsoleHelper.Log("materialScore: " + materialScore + " whitePawnScore: " + whitePawnScore +
                          " blackPawnScore: " + blackPawnScore);
        return materialScore + whitePawnScore + blackPawnScore;
    }
}