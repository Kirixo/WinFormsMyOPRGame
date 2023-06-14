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
    class Enemy
    {
        public delegate void MoveEventHandler(Enemy enemy, Coordinate old, Coordinate now);
        public event MoveEventHandler MoveEvent;
        public event EventHandler<EnemyDieEventArgs> EnemyDie;
        private ManualResetEvent pauseEvent;
        public EnemyBase Base;
        public double HP { get; set; } = 5;
        public int range = 0;
        public double speed { get; set; } = 3;
        public virtual char symbol { get; set; } = 'E';
        public virtual Image img { get; set; } = Properties.Resources.Enemy;
        public bool IsAlive => HP > 0;
        public Coordinate position { get; set; }
        Field field;
        List<Coordinate> path = new List<Coordinate>();
        enum Move
        {
            up,
            left,
            down,
            right
        }
        Dictionary<Move, Coordinate> directions = new Dictionary<Move, Coordinate>()
        {
            { Move.up, new Coordinate(0, -1) },
            { Move.left, new Coordinate(-1, 0) },
            { Move.down, new Coordinate(0, 1) },
            { Move.right, new Coordinate(1, 0) }
        };


        public Enemy(Field field, EnemyBase _base)
        {
            this.field = field;
            Base = _base;
            if (!FindPath((Cell cell) => cell.CanGo))
            {
                FindPath((Cell cell) => cell.CanGo || cell.IsTower);
            }

        }

        public void SetPauseEvent(ManualResetEvent pauseEvent)
        {
            this.pauseEvent = pauseEvent;
        }

        protected void Fight(Tower target)
        {

        }

        public bool FindPath(Func<Cell, bool> canGo)
        {
            Coordinate? start = new Coordinate(Base.X, Base.Y);
            Coordinate? exit = null;
            position = start.Value;

            Queue<Coordinate> queue = new Queue<Coordinate>();
            Dictionary<Coordinate?, Coordinate?> parent = new Dictionary<Coordinate?, Coordinate?>();

            queue.Enqueue(start.Value);
            bool pathFound = false;

            while (queue.Count > 0)
            {
                Coordinate? current = queue.Dequeue();

                if (field.Cells[current.Value.X, current.Value.Y].GetType() == typeof(PlayerBase))
                {
                    exit = current;
                    pathFound = true;
                    break;
                }

                foreach (var direction in directions)
                {
                    Coordinate? next = new Coordinate(
                        current.Value.X + direction.Value.X,
                        current.Value.Y + direction.Value.Y
                    );

                    if (next != null && field.BoundsCheck(next.Value))
                    {
                        Cell nextCell = field.Cells[next.Value.X, next.Value.Y];
                        if (!parent.ContainsKey(next) && !next.Equals(start) && canGo(nextCell))
                        {
                            queue.Enqueue(next.Value);
                            parent[next] = current;
                        }
                    }
                }
            }

            if (pathFound)
            {
                Coordinate? current = exit;
                path = new List<Coordinate>();
                while (current != null)
                {
                    path.Add(current.Value);
                    current = parent.ContainsKey(current) ? parent[current] : null;
                }
                path.Reverse();
                return true;
            }
            return false;
        }

        public async Task moving()
        {
            const int BASE_MOTION_DELAY = 2000;
            foreach (var point in path)
            {
                while (true)
                {
                    pauseEvent.WaitOne();
                    await Task.Delay((int)(BASE_MOTION_DELAY / speed));
                    if (field.Cells[point.X, point.Y].GetType() == typeof(Space) ||
                        field.Cells[point.X, point.Y].GetType() == typeof(EnemyBase))
                    {
                        MoveEvent?.Invoke(this, position, point);
                        position = point;

                        break;
                    }
                    else if (field.Cells[point.X, point.Y].IsTower)
                    {

                    }
                    else if (field.Cells[point.X, point.Y].GetType() == typeof(PlayerBase))
                    {
                        OnEnemyDie(new EnemyDieEventArgs(this));
                        ((PlayerBase)field.Cells[point.X, point.Y]).LivesDecrement(1);
                        break;
                    }
                }
            }
        }

        public void TakingDamage(int Damage)
        {
            HP -= Damage;
            if (HP <= 0)
            {
                OnEnemyDie(new EnemyDieEventArgs(this));
            }
        }

        protected virtual void OnEnemyDie(EnemyDieEventArgs e)
        {
            EnemyDie?.Invoke(this, e);
        }
    }

    class EnemyDieEventArgs : EventArgs
    {
        public Enemy Enemy { get; }


        public EnemyDieEventArgs(Enemy enemy)
        {
            Enemy = enemy;
        }
    }
}
