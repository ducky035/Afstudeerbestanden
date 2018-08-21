using Redzen.Random;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SharpNeat.Domains
{
    public class _2048BlackBoxEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        const double StopFitness = 3867672; // this is the maximum score of a 2048 game
        ulong _evalCount;
        bool _stopConditionSatisfied;

        #region IPhenomeEvaluator<IBlackBox> Members

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox against the 2048 game problem domain and return its fitness score.
        /// </summary>
        public FitnessInfo Evaluate(IBlackBox box)
        {
            ISignalArray inputArr = box.InputSignalArray;
            ISignalArray outputArr = box.OutputSignalArray;

            double fitness = 0;
            IRandomSource _random = RandomSourceFactory.Create();
            int[][] _grid = new int[][] { new int[] { 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0 } };
            int _score = 0;

            // Play 10 2048 games to get rid of lucky games caused by the randomness of the 2048 game
            for (int gameID = 0; gameID < 10; gameID++)
            {
                _evalCount++;

                box.ResetState();

                ResetGrid(_random, _grid, ref _score);

                bool gameOver = false;
                bool invalidTurn = false;
                while (!gameOver)
                {
                    SetInputValues(inputArr, _grid, ref _score);

                    box.Activate();
                    if (!box.IsStateValid)
                    {
                        return FitnessInfo.Zero;
                    }

                    bool? turnResult = DoOneTurn(outputArr, _random, _grid, ref _score);
                    gameOver = !turnResult.HasValue;
                    if(invalidTurn && !gameOver && !turnResult.Value)
                    {
                        break;
                    }
                    invalidTurn = !gameOver && !turnResult.Value;
                }

                fitness += _score;
            }

            fitness /= 10; // average out the fitness

            if (fitness >= StopFitness)
            {
                _stopConditionSatisfied = true;
            }

            return new FitnessInfo(fitness, fitness);
        }

        private bool? DoOneTurn(ISignalArray outputArr, IRandomSource _random, int[][] _grid, ref int _score)
        {
            double[] outputs = new double[4];
            outputArr.CopyTo(outputs, 0);

            double[] highestOutputs = outputs.OrderByDescending(o => o).TakeWhile(o => o == outputs.Max()).ToArray();

            if(highestOutputs.Length == 1)
            {
                return highestOutputs[0] == 0 ? false : Move(Array.IndexOf(outputs, highestOutputs[0]), _random, _grid, ref _score);
            }
            else if(highestOutputs.All(h => h == 0))
            {
                return false;
            }
            else
            {
                int[] highestOutputsIndeces = highestOutputs.Select(h => Array.IndexOf(outputs, h)).ToArray();
                return Move(highestOutputsIndeces[_random.Next(0, highestOutputsIndeces.Length - 1)], _random, _grid, ref _score);
            }
        }

        private bool? Move(int moveID, IRandomSource _random, int[][] _grid, ref int _score)
        {
            switch (moveID)
            {
                case 0: return MoveUp(_random, _grid, ref _score);
                case 1: return MoveDown(_random, _grid, ref _score);
                case 2: return MoveLeft(_random, _grid, ref _score);
                case 3: return MoveRight(_random, _grid, ref _score);
                default: throw new InvalidOperationException($"moveID {moveID} wasn`t expected! Only moveIDs between 0 and 3 are allowed!");
            }
        }

        private bool? MoveUp(IRandomSource _random, int[][] _grid, ref int _score)
        {
            bool[] columnsChanged = new bool[_grid.Length];

            for (int column = 0; column < _grid[0].Length; column++)
            {
                int[] newColumn = new int[] { _grid[0][column], 0, 0, 0 };
                bool[] alreadyMerged = new bool[] { false, false, false, false };

                for (int row = 1; row < _grid[0].Length; row++)
                {
                    int moveToIndex = 0;
                    for (int i = newColumn.Length - 1; i >= 0; i--)
                    {
                        if (newColumn[i] != 0)
                        {
                            moveToIndex = i;
                            break;
                        }
                    }

                    if (newColumn[moveToIndex] == _grid[row][column] && !alreadyMerged[moveToIndex])
                    {
                        newColumn[moveToIndex] *= 2;
                        alreadyMerged[moveToIndex] = true;
                        _score += newColumn[moveToIndex];
                    }
                    else
                    {
                        newColumn[column] = _grid[row][column];
                    }
                }

                columnsChanged[column] = _grid.Select(row => row[column]).Select((tile, i) => newColumn[i] != tile).Any(b => b);
                for (int row = 0; row < _grid.Length; row++)
                {
                    _grid[row][column] = newColumn[row];
                }
            }

            bool isGameOver = _grid.SelectMany(row => row).Any(tile => tile == 0) ||
                              CanGridStillMergeTiles(_grid, ref _score);

            if(columnsChanged.Any(b => b))
            {
                GenerateNewRandomTile(_random, _grid);
            }

            return isGameOver ? null : new bool?(columnsChanged.Any(b => b));
        }

        private bool? MoveDown(IRandomSource _random, int[][] _grid, ref int _score)
        {
            bool[] columnsChanged = new bool[_grid.Length];

            for (int column = 0; column < _grid[0].Length; column++)
            {
                int[] newColumn = new int[] { 0, 0, 0, _grid[_grid.Length - 1][column] };
                bool[] alreadyMerged = new bool[] { false, false, false, false };

                for (int row = _grid.Length - 2; row >= 0; row--)
                {
                    int moveToIndex = newColumn.Length - 1;
                    for (int i = 0; i < newColumn.Length; i++)
                    {
                        if (newColumn[i] != 0)
                        {
                            moveToIndex = i;
                            break;
                        }
                    }

                    if (newColumn[moveToIndex] == _grid[row][column] && !alreadyMerged[moveToIndex])
                    {
                        newColumn[moveToIndex] *= 2;
                        alreadyMerged[moveToIndex] = true;
                        _score += newColumn[moveToIndex];
                    }
                    else
                    {
                        newColumn[column] = _grid[row][column];
                    }
                }

                columnsChanged[column] = _grid.Select(row => row[column]).Select((tile, i) => newColumn[i] != tile).Any(b => b);
                for(int row = 0; row < _grid.Length; row++)
                {
                    _grid[row][column] = newColumn[row];
                }
            }

            bool isGameOver = _grid.SelectMany(row => row).Any(tile => tile == 0) ||
                              CanGridStillMergeTiles(_grid, ref _score);

            if (columnsChanged.Any(b => b))
            {
                GenerateNewRandomTile(_random, _grid);
            }

            return isGameOver ? null : new bool?(columnsChanged.Any(b => b));
        }

        private bool? MoveLeft(IRandomSource _random, int[][] _grid, ref int _score)
        {
            bool[] rowsChanged = new bool[_grid.Length];

            for(int row = 0; row < _grid.Length; row++)
            {
                int[] newRow = new int[] { _grid[row][0], 0, 0, 0 };
                bool[] alreadyMerged = new bool[] { false, false, false, false };

                for (int column = 1; column < _grid[row].Length; column++)
                {
                    int moveToIndex = 0;
                    for(int i = newRow.Length - 1; i >= 0; i--)
                    {
                        if(newRow[i] != 0)
                        {
                            moveToIndex = i;
                            break;
                        }
                    }

                    if (newRow[moveToIndex] == _grid[row][column] && !alreadyMerged[moveToIndex])
                    {
                        newRow[moveToIndex] *= 2;
                        alreadyMerged[moveToIndex] = true;
                        _score += newRow[moveToIndex];
                    }
                    else
                    {
                        newRow[column] = _grid[row][column];
                    }
                }

                rowsChanged[row] = _grid[row].Select((tile, i) => newRow[i] != tile).Any(b => b);
                _grid[row] = newRow;
            }

            bool isGameOver = _grid.SelectMany(row => row).Any(tile => tile == 0) ||
                              CanGridStillMergeTiles(_grid, ref _score);

            if (rowsChanged.Any(b => b))
            {
                GenerateNewRandomTile(_random, _grid);
            }

            return isGameOver ? null : new bool?(rowsChanged.Any(b => b));
        }

        private bool? MoveRight(IRandomSource _random, int[][] _grid, ref int _score)
        {
            bool[] rowsChanged = new bool[_grid.Length];

            for (int row = 0; row < _grid.Length; row++)
            {
                int[] newRow = new int[] { 0, 0, 0, _grid[row][_grid[row].Length - 1] };
                bool[] alreadyMerged = new bool[] { false, false, false, false };

                for (int column = _grid[0].Length - 2; column >= 0; column--)
                {
                    int moveToIndex = newRow.Length - 1;
                    for (int i = 0; i < newRow.Length; i++)
                    {
                        if (newRow[i] != 0)
                        {
                            moveToIndex = i;
                            break;
                        }
                    }

                    if (newRow[moveToIndex] == _grid[row][column] && !alreadyMerged[moveToIndex])
                    {
                        newRow[moveToIndex] *= 2;
                        alreadyMerged[moveToIndex] = true;
                        _score += newRow[moveToIndex];
                    }
                    else
                    {
                        newRow[column] = _grid[row][column];
                    }
                }

                rowsChanged[row] = _grid[row].Select((tile, i) => newRow[i] != tile).Any(b => b);
                _grid[row] = newRow;
            }

            bool isGameOver = _grid.SelectMany(row => row).Any(tile => tile == 0) ||
                              CanGridStillMergeTiles(_grid, ref _score);

            if (rowsChanged.Any(b => b))
            {
                GenerateNewRandomTile(_random, _grid);
            }

            return isGameOver ? null : new bool?(rowsChanged.Any(b => b));
        }

        private bool CanGridStillMergeTiles(int[][] _grid, ref int _score)
        {
            for (int row = 0; row < _grid.Length; row++)
            {
                for(int column = 0; column < _grid[row].Length; column++)
                {
                    int currentTileValue = _grid[row][column];

                    int tileValueLeftOfCurrent = column >= 1 ? _grid[row][column--] : -1;
                    int tileValueRightOfCurrent = column <= _grid[0].Length - 2 ? _grid[row][column++] : -1;
                    int tileValueAboveCurrent = row >= 1 ? _grid[row--][column] : -1;
                    int tileValueBelowCurrent = row <= _grid.Length - 2 ? _grid[row++][column] : -1;

                    if(currentTileValue == tileValueLeftOfCurrent ||
                       currentTileValue == tileValueRightOfCurrent ||
                       currentTileValue == tileValueAboveCurrent ||
                       currentTileValue == tileValueBelowCurrent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetInputValues(ISignalArray inputArr, int[][] _grid, ref int _score)
        {
            Debug.Assert(inputArr.Length == _grid.Sum(row => row.Length), $"The inputArr isn`t the same size as the _grid! inputArr.Length = {inputArr.Length} _grid.Sum(row => row.Length) = {_grid.Sum(row => row.Length)}");

            for(int row = 0; row < _grid.Length; row++)
            {
                for(int column = 0; column < _grid[row].Length; column++)
                {
                    int tileID = row * _grid.Length + column;

                    inputArr[tileID] = TileValueAsFraction(_grid[row][column], _grid, ref _score);
                }
            }
        }

        private double TileValueAsFraction(int tileValue, int[][] _grid, ref int _score)
        {
            int highestTileValue = _grid.SelectMany(row => row).Max();
            return tileValue == 0 ? 0 : (Math.Log(tileValue) / Math.Log(2)) / (Math.Log(highestTileValue) / Math.Log(2));
        }

        private void ResetGrid(IRandomSource _random, int[][] _grid, ref int _score)
        {
            _score = 0;

            for(int row = 0; row < _grid.Length; row++)
            {
                for(int column = 0; column < _grid[row].Length; column++)
                {
                    _grid[row][column] = 0;
                }
            }

            GenerateNewRandomTile(_random, _grid);
            GenerateNewRandomTile(_random, _grid);
        }

        private void GenerateNewRandomTile(IRandomSource _random, int[][] _grid, bool notSoRandom = true)
        {
            if (!notSoRandom)
            {
                int rowID = _random.Next(0, _grid.Length - 1);
                int columnID = _random.Next(0, _grid.Length - 1);
                int tileValue = _random.NextDouble() <= 0.1 ? 4 : 2;
                _grid[rowID][columnID] = tileValue;
            }
            else
            {
                int[] concatenatedTiles = _grid.SelectMany(row => row).ToArray();
                for (int tileIndex = concatenatedTiles.Length - 1; tileIndex >= 0; tileIndex--)
                {
                    if (concatenatedTiles[tileIndex] == 0)
                    {
                        int rowIndex = (int)Math.Ceiling((double)tileIndex / 4) - 1;
                        rowIndex = rowIndex < 0 ? 0 : rowIndex;
                        _grid[rowIndex][tileIndex % 4] = 4;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// Note. The 2048 game problem domain has no internal state. This method does nothing.
        /// </summary>
        public void Reset()
        {
        }

        #endregion
    }
}