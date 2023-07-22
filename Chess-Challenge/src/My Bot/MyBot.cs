using ChessChallenge.API;
using ChessChallenge.Application;

public class MyBot : IChessBot
{
    private int _maxDepth = 3;
    
    // Avoid allocating memory by storing the board as a field
    // It also avoid spending extra tokens on passing the board as a parameter
    private Board _board;
    private int[] _pieceWeights = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        _board = board;
        
        Move bestMove = new();
        int bestScore = int.MinValue;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -NegaMax(_maxDepth);
            board.UndoMove(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int NegaMax(int depth)
    {
        if (depth == 0)
            return EvaluateMaterial();

        int bestScore = int.MinValue;
        foreach (Move move in _board.GetLegalMoves())
        {
            _board.MakeMove(move);
            int score = -NegaMax(depth - 1);
            _board.UndoMove(move);
            if (score > bestScore)
                bestScore = score;
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