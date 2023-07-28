using ChessChallenge.API;
using System;
using System.Linq;
using System.Collections.Generic;
namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        
        // myBoutv1
        public int EvaluateSquareLocation(Piece piece){

            switch ((int)piece.PieceType)
            {
                case 1:
                    int[] pawnTable = {
                        0, 0, 0, 0, 0, 0, 0, 0,
                        50, 50, 50, 50, 50, 50, 50, 50,
                        10, 10, 20, 30, 30, 20, 10, 10,
                        5, 5, 10, 25, 25, 10, 5, 5,
                        0, 0, 0, 20, 20, 0, 0, 0,
                        5, -5, -10, 0, 0, -10, -5, 5,
                        5, 10, 10, -20, -20, 10, 10, 5,
                        0, 0, 0, 0, 0, 0, 0, 0
                    };
                    return (piece.IsWhite ? pawnTable[piece.Square.Index] : pawnTable[63 - piece.Square.Index]);

                case 2:
                    return 0;
                case 3:
                    return 0;
                case 4:
                    return 0;
                case 5:
                    return 0;
                case 6:
                
                    return 0;
                default:
                    return 0;

            }
        }


        public int Evaluate(Board board)
        {

            int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
            
            int total = 0;
            PieceList[] pieceListArray = board.GetAllPieceLists();

            for (int i = 0; i < 12; i++){

                PieceList pieceList = pieceListArray[i];
                foreach (Piece piece in pieceList){

                    int score = pieceValues[(int)piece.PieceType];
                    if (i < 6){
                        total += (board.IsWhiteToMove ? score : -score);
                    } 
                    else {
                        total += (board.IsWhiteToMove ? -score : score);
                    }

                    total += EvaluateSquareLocation(piece);
                
                }
            }

            return total;
        }


        public int EvaluationSearch(Board board, int depth, int depthStart, ref Move bestMove, int alpha, int beta, DateTime startTime, ref Move[] allMovesOrdered)
        {

            if (board.IsInCheckmate()){
                return int.MinValue + 1;
            }
            if (board.IsDraw()){
                return 0;
            }
            


            Move[] allMoves;
            if (depth == depthStart){
                allMoves = allMovesOrdered;
            }
            else{
                if(depth <= 0){
                    allMoves = board.GetLegalMoves(true);
                    int score = Evaluate(board);
                    if (score >= beta){
                        return beta;
                    }
                    alpha = Math.Max(alpha, score);

                    if (allMoves.Length == 0){
                        return Evaluate(board);
                    }

                }else{
                    allMoves = board.GetLegalMoves(false);
                }
            }

            // to be used if at first death of search
            Dictionary<Move, int> moveScores = new Dictionary<Move, int>();

            foreach (Move move in allMoves){

                board.MakeMove(move);
                int score = -EvaluationSearch(board, depth - 1, depthStart, ref bestMove, -beta, -alpha, startTime, ref allMovesOrdered);
                board.UndoMove(move);

                // kill the search if time limit surpassed
                if (score == -1 || (DateTime.Now - startTime).TotalSeconds > 0.4)         
                {
                    return -1;
                }

                // if we are at our first depth, add the move and score to the dictionary
                if (depthStart == depth){
                    moveScores.Add(move, score);
                }
                
                if (score >= beta){
                    return beta;
                }

                if (score > alpha){
                    alpha = score;
                    if(depth == depthStart) {
                        bestMove = move;
                    
                    }
                }
            }
            // if we are at first depth, order the moves by their score to increase prunes in following deeper searches
            if (depthStart == depth){
                allMovesOrdered = moveScores.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
                
            }
            
            return alpha;
        }

        public Move Think(Board board, Timer timer)
        {

            Move bestMove = new Move();
            Move[] orderedMoves = board.GetLegalMoves(false);
            int alpha = -100000;
            int beta = 100000;
            int depth = 1;


            while (true){
                Move bestMoveTemp = new Move();
                int searchCompleted = EvaluationSearch(board, depth, depth, ref bestMoveTemp, alpha, beta, DateTime.Now, ref orderedMoves);
                if (searchCompleted == -1){
                    break;
                }
                bestMove = bestMoveTemp;
                depth++;

            }

            // Console.WriteLine("evil Bot Depth: " + (depth - 1));

            return bestMove;

        }
    }
}