using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOPRGame
{
    
    class MapGenerator
    {
        enum CellState
        {
            Floor,
            Wall,
            Chaos
        }

        private class cell
        {
            public CellState State { get; set; }
            public CellState NewState { get; set; }

            public cell()
            {
                State = CellState.Chaos;
            }

            public cell(bool isWall)
            {
                State = isWall ? CellState.Wall : CellState.Floor;
                NewState = State;
            }
        }

        private int _width, _height;
        private cell[,] _field;
        public char[][] ReField;

        public MapGenerator(int width, int height)
        {
            _width = width;
            _height = height;
            _field = new cell[width, height];
            ReField = new char[height][];
            for(int i = 0; i<height; i++)
            {
                ReField[i] = new char[width];
            }
        }

        public void Generate()
        {
            Random random = new Random();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _field[x, y] = new cell();
                }
            }
            for (int i = 1; i < _width - 1; i++)
            {
                for (int j = 1; j < _height - 1; j++)
                {
                    if (_field[i, j].State != CellState.Chaos)
                    {
                        continue;
                    }
                    _field[i > 1 ? i - 1 : i, j].State = CellState.Floor;

                    bool isStable = false;
                    while (!isStable)
                    {
                        isStable = true;


                        for (int x = 1; x < _width - 1; x++)
                        {
                            for (int y = 1; y < _height - 1; y++)
                            {
                                cell cell = _field[x, y];
                                if (cell.State == CellState.Chaos)
                                {
                                    List<cell> neighbors = new List<cell>
                                    {
                                        _field[x + 1, y], // Сосед справа
                                        _field[x - 1, y], // Сосед слева
                                        _field[x, y - 1], // Сосед сверху
                                        _field[x, y + 1] // Сосед снизу
                                    };

                                    if (neighbors.Exists(c => c.State == CellState.Floor))
                                    {
                                        if (neighbors.Exists(c => c.State == CellState.Wall))
                                        {
                                            cell.NewState = random.NextDouble() < 0.25 ? CellState.Wall : CellState.Floor;
                                        }
                                        else
                                        {
                                            cell.NewState = random.NextDouble() < 0.7 ? CellState.Floor : CellState.Wall;
                                        }

                                        if (cell.State != cell.NewState)
                                        {
                                            cell.State = cell.NewState;
                                            isStable = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }



            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    cell cell = _field[x, y];
                    ReField[y][x] = cell.State == CellState.Wall ? '#' : cell.State == CellState.Floor ? '0' : '#';
                }
            }

            BaseArranger(ref ReField);
        }

        private void BaseArranger(ref char[][] map)
        {
            Random random = new Random();

            Coordinate PlayerBase = new Coordinate(random.Next(1, _width - 1), random.Next(1, _height - 1));
            Coordinate EnemyBase = new Coordinate(random.Next(1, _width - 1), random.Next(1, _height - 1));
            while (PlayerBase.Distance(EnemyBase) < Math.Max(_width/2, _height/2))
            {
                EnemyBase = new Coordinate(random.Next(1, _width - 1), random.Next(1, _height - 1));
            }



            map[EnemyBase.Y][EnemyBase.X] = '!';
            map[PlayerBase.Y][PlayerBase.X] = 'X';
        }
        
    }
}
