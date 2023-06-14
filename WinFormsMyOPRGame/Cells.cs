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
    abstract class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public virtual ConsoleColor color { get; set; }
        public virtual bool CanGo { get; set; }
        public virtual bool IsTower { get; set; } = false;
        public virtual char symbol { get; set; }
        public virtual Image img => Properties.Resources.Wall;


        protected Cell(int posX, int posY)
        {
            X = posX;
            Y = posY;
        }
    }

    class Space : Cell
    {
        public override bool CanGo => true;
        public override char symbol => ' ';
        public override Image img => Properties.Resources.Floor;
        public override ConsoleColor color => ConsoleColor.Black;


        public Space(int posX, int posY) : base(posX, posY)
        {
        }
    }

    class Wall : Cell
    {
        public override bool CanGo => false;
        public override char symbol => '#';
        public override Image img => Properties.Resources.Wall2;
        public override ConsoleColor color => ConsoleColor.White;


        public Wall(int posX, int posY) : base(posX, posY)
        {
        }
    }

    abstract class Base : Cell
    {
        public Base(int posX, int posY) : base(posX, posY)
        {
        }
    }

    class EnemyBase : Base
    {
        public override bool CanGo => true;
        public override char symbol => '!';
        public override ConsoleColor color => ConsoleColor.Red;
        public override Image img => Properties.Resources.Portal1;
        public int ENEMY_COUNT = 5;
        public int ENEMY_INTERVAL = 15000;

        public EnemyBase(int posX, int posY, Field field) : base(posX, posY)
        {
            EnemySpawner(field);
        }

        public void EnemySpawner(Field field)
        {
            Thread enemyThread = new Thread(() =>
            {
                for (int i = 0; i < ENEMY_COUNT; i++)
                {
                    Enemy enemy = new Enemy(field, this);
                    field.AddingEnemies(enemy);
                    Thread.Sleep(ENEMY_INTERVAL);
                }
            });
            enemyThread.Start();
        }

    }

    class PlayerBase : Base
    {
        public delegate void PlayerLost();
        public event PlayerLost PlayerLostEvent;
        public override bool CanGo => true;
        public override char symbol => '~';
        public override ConsoleColor color => ConsoleColor.Blue;
        public override Image img => Properties.Resources.Portal2;
        public int Lives { get; set; } = 4;


        public PlayerBase(int posX, int posY) : base(posX, posY)
        {
        }

        public void LivesDecrement(int decrementValue)
        {
            Lives -= decrementValue;
            if (Lives <= 0)
            {
                PlayerLostEvent.Invoke();
            }
        }
    }
}
