using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;

namespace MyOPRGame
{
    class Game
    {
        MainMenu menu;
        protected Dictionary<string, Action> menuItems;
        Drawer drawer;

        public Game(Drawer drawer)
        {
            this.drawer = drawer;
            menuItems = new Dictionary<string, Action>()
                {
                    {"New Game", NewGame },
                    {"Exit", Exit }
                };
            menu = new MainMenu(drawer, menuItems);
        }

        public void NewGame()
        {
            GameProcess process = new GameProcess(drawer, 1);
        }

        protected void Exit()
        {
            Environment.Exit(0);
        }

    }

    class GameProcess : Game
    {
        Field field;
        Drawer drawer;
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);
        UserInput userInput;


        public GameProcess(Drawer drawer, int lvlID) : base(drawer)
        {
            this.drawer = drawer;
            FieldCreation(lvlID);
            userInput = new UserInput(drawer, field);
            userInput.StartUserInput();
        }

        private string GetTextResourceLines(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            ResourceManager resourceManager = new ResourceManager(assembly.GetName().Name + ".Properties.Resources", assembly);

            string resourceObject = resourceManager.GetString(resourceName);

            return resourceObject;
        }

        void ReadLvlInfo(ref char[][] lvlmap, int lvlID)
        {
            string resourceName = $"lvl{lvlID}";
            string resourceText = GetTextResourceLines(resourceName);

            if (resourceText != null)
            {
                string[] lines = resourceText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                lvlmap = new char[lines.Length][];
                for (int i = 0; i < lines.Length; i++)
                {
                    lvlmap[i] = lines[i].ToCharArray();
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }

        void FieldCreation(int lvlID)
        {
            char[][] arrayfield = null;
            if(lvlID < 0)
            {
                MapGenerator mapGenerator = new MapGenerator(40, 10);
                mapGenerator.Generate();
                arrayfield = mapGenerator.ReField;
            }
            ReadLvlInfo(ref arrayfield, lvlID);
            field = new Field(arrayfield[0].Length, arrayfield.Length, drawer);
            field.FillingField(arrayfield);
            field.InitPauseEvent(pauseEvent);
            field.PlayerLostEvent += PlayerLost;
            field.PlayerWonEvent += PlayerWon;
            drawer.DrawField(field);
        }

        public void GamePause()
        {
            pauseEvent.Reset();
        }

        public void ResumeGame()
        {
            pauseEvent.Set();
        }

        public void PlayerLost()
        {
            GamePause();
            Thread.Sleep(1000);
            drawer.DrawLost();
        }

        public void PlayerWon()
        {
            GamePause();
            Thread.Sleep(1000);
            drawer.DrawWin();
        }
    }
}