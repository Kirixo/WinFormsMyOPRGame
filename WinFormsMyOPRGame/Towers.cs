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
    abstract class Tower : Cell
    {
        public override bool CanGo => false;
        protected int ATK = 1;
        protected int Range = 1;
        protected int HP = 10;
        public string type { get; set; }
        public override bool IsTower => true;
        public Cell MapElementAtThisPoint { get; set; }
        private List<Enemy> enemiesInRange;
        Field field;


        public Tower(int posX, int posY, Field field) : base(posX, posY)
        {
            this.field = field;
            EnemySearch();
        }

        private async Task EnemySearch()
        {
            const int ENEMY_SEARCH_INTERVAL = 5000;
            while (true)
            {
                await Task.Delay(ENEMY_SEARCH_INTERVAL);
                enemiesInRange = field.GetCellsInRange(this, this.Range);
                if (enemiesInRange.Count() != 0)
                {
                    Fight(enemiesInRange[0]);
                }
            }
        }

        public void Fight(Enemy enemy)
        {
            enemy.TakingDamage(ATK);
        }

        public void TakingDamage(int Damage)
        {
            HP -= Damage;
            if (HP <= 0)
            {
                //OnEnemyDie(new EnemyDieEventArgs(this));
            }
        }

        public abstract bool IsValidPosition(Cell cell);
    }

    class StrongTower : Tower
    {
        public override char symbol => 'S';
        public override Image img => Properties.Resources.StrongTower;


        public StrongTower(int posX, int posY, Field field) : base(posX, posY, field)
        {
            type = "ground";
            ATK = 2;
        }

        public override bool IsValidPosition(Cell cell)
        {
            return cell.GetType() == typeof(Space);
        }
    }

    class RangeTower : Tower
    {
        public override char symbol => 'R';
        public override Image img => Properties.Resources.RangeTower;


        public RangeTower(int posX, int posY, Field field) : base(posX, posY, field)
        {
            type = "wall";
            Range = 2;
        }

        public override bool IsValidPosition(Cell cell)
        {
            return cell.GetType() == typeof(Wall);
        }
    }

    class StrongRangeTower : Tower
    {
        public override char symbol => 'U';
        public override Image img => Properties.Resources.StrongRangeTower;


        public StrongRangeTower(int posX, int posY, Field field) : base(posX, posY, field)
        {
            type = "combined";
            ATK = 2;
            Range = 2;
        }

        public override bool IsValidPosition(Cell cell)
        {
            return cell.GetType() == typeof(Space) || cell.GetType() == typeof(Wall);
        }
    }
}
