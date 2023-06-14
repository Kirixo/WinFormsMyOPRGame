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
    abstract class Menu
    {
        public Dictionary<string, Action> MenuItems;
        public abstract string[] OptionsList { get; set; }


        public Menu(Drawer drawer, Dictionary<string, Action> _menuItems)
        {
            drawer.DrawMenu(this);
            MenuItems = _menuItems;
            int optionID = MenuSelector.SelectMenuItem(OptionsList);
            if (optionID >= 0)
            {
                OptionChecker(OptionsList[optionID]);
            }
        }

        public void OptionChecker(string option)
        {
            MenuItems[option].Invoke();
        }
    }

    class MainMenu : Menu
    {
        string[] options = new string[] { "New Game", "Exit", };
        public override string[] OptionsList { get => options; set => OptionsList = options; }


        public MainMenu(Drawer drawer, Dictionary<string, Action> menuItems) : base(drawer, menuItems)
        {
        }
    }

    class SideMenu : Menu
    {
        GameProcess currentGame;
        string[] options = new string[] { "Continue", "New Game", "Exit", };
        public override string[] OptionsList { get => options; set => OptionsList = options; }


        public SideMenu(Drawer drawer, Dictionary<string, Action> menuItems, GameProcess game) : base(drawer, menuItems)
        {
            currentGame = game;
        }

    }
}
