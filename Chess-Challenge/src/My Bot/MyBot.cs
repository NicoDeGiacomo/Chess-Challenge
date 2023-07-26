using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private const int MaxDepth = 3;
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

        Move bestMove = new();
        int bestScore = -int.MaxValue;
        int alpha = -int.MaxValue;
        int beta = int.MaxValue;

        Span<Move> moves = stackalloc Move[256];
        _board.GetLegalMovesNonAlloc(ref moves);
        foreach (Move move in moves)
        {
            _board.MakeMove(move);
            int score = -AlphaBeta(-beta, -alpha, MaxDepth - 1);
            _board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            if (score > alpha)
                alpha = score;
        }


        // Null move safeguard
        return bestMove == Move.NullMove ? moves[0] : bestMove;
    }

    private int AlphaBeta(int alpha, int beta, int depth)
    {
        // ConsoleHelper.Log("AlphaBeta - " + alpha + " " + beta + " " + depth);
        bool principalVariationFound = false;
        if (depth == 0)
            return Quiesce(alpha, beta);

        Span<Move> moves = stackalloc Move[256];
        _board.GetLegalMovesNonAlloc(ref moves);
        foreach (Move move in moves)
        {
            _board.MakeMove(move);
            int score;
            if (principalVariationFound)
            {
                score = -AlphaBeta(-alpha - 1, -alpha, depth - 1);
                if (score > alpha && score < beta)
                    score = -AlphaBeta(-beta, -alpha, depth - 1);
            }
            else
                score = -AlphaBeta(-beta, -alpha, depth - 1);

            _board.UndoMove(move);

            if (score >= beta)
                return beta;

            if (score > alpha)
            {
                alpha = score;
                principalVariationFound = true;
            }
        }

        return alpha;
    }

    private int Quiesce(int alpha, int beta)
    {
        // ConsoleHelper.Log("Quiesce - " + alpha + " " + beta);
        int standPat = Evaluate();
        if (standPat >= beta)
            return beta;
        if (alpha < standPat)
            alpha = standPat;

        Span<Move> moves = stackalloc Move[256];
        _board.GetLegalMovesNonAlloc(ref moves, true);
        foreach (Move move in moves)
        {
            _board.MakeMove(move);
            int score = -Quiesce(-beta, -alpha);
            _board.UndoMove(move);

            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }

        return alpha;
    }

    private int Evaluate()
    {
        if (_board.IsInCheckmate())
            return (_board.IsWhiteToMove ? -20000 : 20000);
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

        // ConsoleHelper.Log("Evaluation: " + (materialScore + whitePawnScore + blackPawnScore) * (_board.IsWhiteToMove ? 1 : -1));
        return (materialScore + whitePawnScore + blackPawnScore) * (_board.IsWhiteToMove ? 1 : -1);
    }
}