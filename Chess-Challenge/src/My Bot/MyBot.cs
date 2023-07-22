using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using ChessChallenge.Application;

public class MyBot : IChessBot
{
    private int _maxDepth = 0;

    // Used to save tokens and avoid memory allocation.
    private Board _board;
    private readonly int[] _pieceWeights = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        _board = board;

        // Negamax algorithm
        Move bestMove = new();
        int bestScore = int.MinValue;
        foreach (Move move in _board.GetLegalMoves())
        {
            _board.MakeMove(move);
            int score = -AlpaBeta(int.MinValue, int.MaxValue, _maxDepth);
            _board.UndoMove(move);
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
        if (depth == 0)
            return QuiescenceSearch(alpha, beta);

        foreach (Move move in _board.GetLegalMoves())
        {
            _board.MakeMove(move);
            int score = -AlpaBeta(-beta, -alpha, depth - 1);
            _board.UndoMove(move);
            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }

        return alpha;
    }

    private int QuiescenceSearch(int alpha, int beta)
    {
        int standingPat = Evaluate();
        if( standingPat >= beta )
            return beta;
        if( alpha < standingPat )
            alpha = standingPat;

        foreach (var move in _board.GetLegalMoves().Where(x => x.IsCapture))
        {
            _board.MakeMove(move);
            int score = -QuiescenceSearch(-beta, -alpha);
            _board.UndoMove(move);
            
            if( score >= beta )
                return beta;
            if( score > alpha )
                alpha = score;
        }
        
        return alpha;
    }

    private int Evaluate()
    {
        int score = 0;
        foreach (PieceList pieceList in _board.GetAllPieceLists())
        foreach (Piece piece in pieceList)
            score += (piece.IsWhite ? 1 : -1) * _pieceWeights[(int)piece.PieceType];

        return score * (_board.IsWhiteToMove ? 1 : -1);
        // return score;
    }
}