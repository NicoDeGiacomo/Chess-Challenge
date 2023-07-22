using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using ChessChallenge.Application;

public class MyBot : IChessBot
{
    private int _maxDepth = 3;

    // Used to save tokens and avoid memory allocation.
    private Board _board;
    private readonly int[] _pieceWeights = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        _board = board;
        //_pieces = board.GetAllPieceLists().SelectMany(x => x).ToList();
        //_moves = board.GetLegalMoves().ToList();

        // Negamax algorithm
        Move bestMove = new();
        int bestScore = int.MinValue;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -AlpaBeta(int.MinValue, int.MaxValue, _maxDepth);
            board.UndoMove(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int AlpaBeta(int alpha, int beta, int depth)
    {
        int bestScore = int.MinValue;
        if (depth == 0)
            return EvaluateMaterial();

        foreach (Move move in _board.GetLegalMoves())
        {
            _board.MakeMove(move);
            int score = -AlpaBeta(-alpha, -beta, depth - 1);
            _board.UndoMove(move);
            // fail-soft beta-cutoff
            // if (score >= beta)
                // return score;
            if (score > bestScore)
            {
                bestScore = score;
                if (score > alpha)
                    alpha = score;
            }
        }

        return bestScore;
    }

    private int EvaluateMaterial()
    {
        int score = 0;
        foreach (PieceList pieceList in _board.GetAllPieceLists())
        foreach (Piece piece in pieceList)
            score += (piece.IsWhite ? 1 : -1) * _pieceWeights[(int)piece.PieceType];

        return score * (_board.IsWhiteToMove ? 1 : -1);
        // return score;
    }
}