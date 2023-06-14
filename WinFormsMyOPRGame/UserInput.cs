using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace MyOPRGame
{
    class UserInput
    {
        Field field;
        Drawer drawer;
        public UserInput(Drawer drawer, Field field)
        {
            this.field = field;
            this.drawer = drawer;
        }

        public void StartUserInput()
        {
            initTowersButtons();
            drawer.DrawTowerSelectBox(field);
        }

        private void initTowersButtons()
        {
            drawer.towerButtons = new Drawer.nonFocusedButton[field.availableTowers.Length];
            for(int i = 0; i<field.availableTowers.Length; i++)
            {
                int towerID = i;
                drawer.towerButtons[i] = new Drawer.nonFocusedButton();
                drawer.towerButtons[i].Click += (sender, e) => {
                    GetCoordinate(towerID);
                };
            }
        }

        public void GetCoordinate(int TowerID)
        {
            Coordinate cursor = field.availableTowers[TowerID].X < 0 ? new Coordinate(0, 0) : new Coordinate(field.availableTowers[TowerID].X, field.availableTowers[TowerID].Y);
            drawer.DrawPic(cursor, drawer.tempImg(field.availableTowers[TowerID].img));

            KeyEventHandler keyDownHandler = null;

            keyDownHandler = (sender, e) =>
            {
                drawer.DrawCeil(field, cursor);
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        cursor.Y = Math.Max(0, cursor.Y - 1);
                        break;
                    case Keys.Down:
                        cursor.Y = Math.Min(field.Height - 1, cursor.Y + 1);
                        break;
                    case Keys.Left:
                        cursor.X = Math.Max(0, cursor.X - 1);
                        break;
                    case Keys.Right:
                        cursor.X = Math.Min(field.Width - 1, cursor.X + 1);
                        break;
                    case Keys.Enter:
                        if (field.availableTowers[TowerID].IsValidPosition(field.Cells[cursor.X, cursor.Y]))
                        {
                            field.InstallTower(TowerID, new Coordinate(cursor.X, cursor.Y));
                        }
                        drawer.form.KeyDown -= keyDownHandler;
                        return;
                }
                drawer.DrawPic(cursor, drawer.tempImg(field.availableTowers[TowerID].img));
            };

            drawer.form.KeyDown += keyDownHandler;
        }
    }
}