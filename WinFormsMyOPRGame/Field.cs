using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace MyOPRGame
{
    class Field
    {
        public int Width;
        public int Height;
        public Cell[,] Cells = null;
        public Tower[] availableTowers;
        public List<Enemy>[,] EnemiesMap;
        public delegate void GameStatus();
        public event GameStatus PlayerLostEvent;
        public event GameStatus PlayerWonEvent;
        public ManualResetEvent PauseEvent;
        public int TotalEnemyCount { get; set; } = 0;
        Drawer drawer;
        private int _remainingEnemyCount;
        int RemainingEnemyCount
        {
            get
            {
                return _remainingEnemyCount;
            }
            set
            {
                if (value <= 0)
                {
                    PlayerWonEvent.Invoke();
                }
                _remainingEnemyCount = value;
            }
        }

        public Field(int width, int height, Drawer _drawer)
        {
            availableTowers = new Tower[] {
                    new StrongTower(-1, -1, this),
                    new RangeTower(-1, -1, this),
                    new StrongRangeTower(-1, -1, this)
                };
            Width = width;
            Height = height;
            drawer = _drawer;
            Cells = new Cell[width, height];
            EnemiesMap = new List<Enemy>[Cells.GetLength(0), Cells.GetLength(1)];
            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    EnemiesMap[i, j] = new List<Enemy>();
                }
            }
        }

        public void FillingField(char[][] inputfield)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    switch (inputfield[i][j])
                    {
                        case '#':
                            Cells[j, i] = new Wall(j, i);
                            break;
                        case '0':
                            Cells[j, i] = new Space(j, i);
                            break;
                        case '!':
                            Cells[j, i] = new EnemyBase(j, i, this);
                            TotalEnemyCount += ((EnemyBase)Cells[j, i]).ENEMY_COUNT;
                            break;
                        case 'X':
                            PlayerBase playerBase = new PlayerBase(j, i);
                            playerBase.PlayerLostEvent += () => PlayerLostEvent.Invoke();
                            Cells[j, i] = playerBase;

                            break;
                        default:
                            //Console.WriteLine($"Developer is stupid. As always. Ceil ({inputfield[i][j]}) does not exist, so at ({j + 1},{Height - i}) will be wall!");
                            Cells[j, i] = new Wall(j, i);
                            break;
                    }
                }
            }
            RemainingEnemyCount = TotalEnemyCount;
        }

        public void InitPauseEvent(ManualResetEvent pauseEvent)
        {
            PauseEvent = pauseEvent;
        }

        public void AddingEnemies(Enemy enemy)
        {
            int x = enemy.Base.X;
            int y = enemy.Base.Y;
            enemy.MoveEvent += MoveEnemy;
            enemy.EnemyDie += EnemyDie;
            enemy.SetPauseEvent(PauseEvent);
            enemy.moving();
            EnemiesMap[x, y].Add(enemy);
        }

        public bool BoundsCheck(Coordinate coord)
        {
            return (coord.X >= 0 && coord.X < Cells.GetLength(0) &&
                    coord.Y >= 0 && coord.Y < Cells.GetLength(1));
        }

        public void InstallTower(int TowerID, Coordinate position)
        {
            if (availableTowers[TowerID].X != -1)
            {
                Cells[availableTowers[TowerID].X, availableTowers[TowerID].Y] = availableTowers[TowerID].MapElementAtThisPoint;
                drawer.DrawCeil(this, new Coordinate(availableTowers[TowerID].X, availableTowers[TowerID].Y));
            }
            availableTowers[TowerID].X = position.X;
            availableTowers[TowerID].Y = position.Y;
            availableTowers[TowerID].MapElementAtThisPoint = Cells[position.X, position.Y];
            Cells[position.X, position.Y] = availableTowers[TowerID];

            drawer.DrawCeil(this, position);
        }

        public List<Enemy> GetCellsInRange(Cell centerCell, int range)
        {
            List<Enemy> cellsInRange = new List<Enemy>();
            int startX = Math.Max(centerCell.X - range, 0);
            int endX = Math.Min(centerCell.X + range, Width - 1);
            int startY = Math.Max(centerCell.Y - range, 0);
            int endY = Math.Min(centerCell.Y + range, Height - 1);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    int dx = Math.Abs(centerCell.X - x);
                    int dy = Math.Abs(centerCell.Y - y);
                    if (dx + dy <= range && EnemiesMap[x, y] != null)
                    {
                        foreach (var enemy in EnemiesMap[x, y])
                        {
                            cellsInRange.Add(enemy);
                        }

                    }
                }
            }

            return cellsInRange;
        }

        public void MoveEnemy(Enemy enemy, Coordinate old, Coordinate now)
        {
            EnemiesMap[enemy.position.X, enemy.position.Y].Remove(enemy);
            EnemiesMap[now.X, now.Y].Add(enemy);
            drawer.DrawEnemie(this, old, now);
        }

        private void EnemyDie(object sender, EnemyDieEventArgs e)
        {
            Enemy enemy = e.Enemy;
            int x = enemy.position.X;
            int y = enemy.position.Y;
            EnemiesMap[x, y].Remove(enemy);
            drawer.DrawCeil(this, new Coordinate(x, y));
            RemainingEnemyCount--;
        }
    }
}
