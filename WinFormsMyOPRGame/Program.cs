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

namespace WinFormsMyOPRGame
{
    struct Coordinate
    {
        public int X;
        public int Y;


        public Coordinate(int x = -1, int y = -1)
        {
            X = x;
            Y = y;
        }
    }
    class Game
    {
        MainMenu menu;
        Form form;
        protected Dictionary<string, Action> menuItems;


        public Game(in Form form) { 
            this.form = form;
            menuItems = new Dictionary<string, Action>()
            {
                {"New Game", NewGame },
                {"Exit", Exit }
            };
            menu = new MainMenu(form, menuItems);
        }

        public void NewGame()
        {
            GameProcess process = new GameProcess(form, 1);
        }

        protected void Exit()
        {
            Environment.Exit(0);
        }

    }

    class GameProcess
    {
        Form form;
        Field field;
        Drawer drawer;
        Thread enemyThread;
        Thread userPrompts;
        private ManualResetEvent pauseEvent = new ManualResetEvent(true);


        public GameProcess(Form form, int lvlID)
        {
            this.form = form;
            drawer = new Drawer(form);
            FieldCreation(lvlID);
        }

        void ReadLvlInfo(ref char[][] lvlmap, int lvlID)
        {
            string txtFile = $"lvl{lvlID}.txt";
            string[] text = File.ReadAllLines(txtFile);
            lvlmap = new char[text.Length][];
            for (int i = 0; i < text.Length; i++)
            {
                lvlmap[i] = text[i].ToCharArray();
            }
        }

        void FieldCreation(int lvlID)
        {
            char[][] arrayfield = null;
            ReadLvlInfo(ref arrayfield, lvlID);
            field = new Field(arrayfield[0].Length, arrayfield.Length);
            field.FillingField(arrayfield);
            drawer.DrawField(field);
            //
            //drawer.DrawScore(field);
            //Для генерації ворогів десь повинна бути окрема функція
            const int ENEMY_COUNT = 5;
            const int ENEMY_INTERVAL = 15000;
            enemyThread = new Thread(() => {
                for (int i = 0; i < ENEMY_COUNT; i++)
                {
                    Enemy enemy = new Enemy(field.Cells);
                    field.AddingEnemies(enemy, 0, 1);
                    Thread.Sleep(ENEMY_INTERVAL);
                }
            });
            enemyThread.Start();

            userPrompts = new Thread(() => {
                Gaming();
            });
            userPrompts.Start();

            Thread KeysInput = new Thread(() => {
                /*
                Thread.Sleep(2);
                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        GamePause();
                    }
                }*/
            });
            KeysInput.Start();

            Thread renderingThread = new Thread(() => {
                Drawer drawer = new Drawer(form);
                const int RENDER_INTERVAL = 1000;
                while (true)
                {
                    drawer.DrawEnemies(field);
                    Thread.Sleep(RENDER_INTERVAL);
                }
            });
            renderingThread.Start();
        }

        public void GamePause()
        {
            Console.WriteLine("XXXXXXXXXX");
        }
        public void ResumeGame()
        {
            pauseEvent.Set();
        }

        public void Gaming()
        {
            while (true)
            {
                Console.WriteLine($"Select Tower that you want to set.");
                int TowerID = GetTowerId();
                Coordinate position = GetCoordinate();
                if (IsValidPosition(position, TowerID))
                {
                    if (field.availableTowers[TowerID - 1].X != -1)
                    {
                        field.Cells[field.availableTowers[TowerID - 1].X, field.availableTowers[TowerID - 1].Y] = field.availableTowers[TowerID - 1].MapElementAtThisPoint;
                    }
                    field.availableTowers[TowerID - 1].X = position.X;
                    field.availableTowers[TowerID - 1].Y = position.Y;
                    field.availableTowers[TowerID - 1].MapElementAtThisPoint = field.Cells[position.X, position.Y];
                    field.Cells[position.X, position.Y] = field.availableTowers[TowerID - 1];

                    drawer.DrawField(field);
                }
                else
                {
                    Console.WriteLine($"Ceil {position.X}, {position.Y} does not exist. Please select available coordinates.");
                }
            }
        }

        

        private int GetTowerId()
        {
            int towerId;

            while (true)
            {
                Console.WriteLine($"Select Tower ID (from 1 to {field.availableTowers.Length}):");
                string ReadLine = Console.ReadLine();
                if (int.TryParse(ReadLine, out towerId) && towerId > 0 && towerId <= field.availableTowers.Length)
                {
                    return towerId;
                }
                Console.WriteLine($"Invalid input. {ReadLine} Please enter a valid Tower ID.");
                /*int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);
                // Очистка одной строки. Когда ни будь разберусь и допилю. 
                */
            }
        }

        private bool IsValidPosition(Coordinate position, int towerID)
        {
            var CellType = field.Cells[position.X, position.Y].GetType();
            switch (field.availableTowers[towerID - 1].type)
            {
                case "ground":
                    if (CellType != typeof(Space))
                    {
                        return false;
                    }
                    break;
                case "wall":
                    if (CellType != typeof(Wall))
                    {
                        return false;
                    }
                    break;
                case "combined":
                    return CellType == typeof(Wall) || CellType == typeof(Space);
                    //TODO
            }
            return true;
        }

        private Coordinate GetCoordinate()
        {
            Coordinate cursor = new Coordinate(0, 0);
            int currentLineCursor;
            while (true)
            {
                Coordinate currentCoordinate = cursor;
                char currentSymbol = field.Cells[cursor.X, cursor.Y].symbol;
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (cursor.Y > 0)
                        {
                            cursor.Y--;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (cursor.Y < field.Height - 1)
                        {
                            cursor.Y++;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (cursor.X > 0)
                        {
                            cursor.X--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (cursor.X < field.Width - 1)
                        {
                            cursor.X++;
                        }
                        break;
                    case ConsoleKey.Enter:
                        currentLineCursor = Console.CursorTop;
                        Console.SetCursorPosition(currentCoordinate.X, currentCoordinate.Y);
                        Console.Write(currentSymbol);
                        Console.SetCursorPosition(0, currentLineCursor);
                        return cursor;
                }
                //Console.WriteLine($"{cursor.X},{cursor.Y}");
                currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(currentCoordinate.X, currentCoordinate.Y);
                Console.Write(currentSymbol);
                Console.SetCursorPosition(cursor.X, cursor.Y);
                Console.Write('?');
                Console.SetCursorPosition(0, currentLineCursor);
            }
        }
    }

    class Drawer
    {
        Form form;
        public Drawer(Form form)
        {
            this.form = form;
        }

        public void DrawField(Field field)
        {
            form.Controls.Clear();
            int cellSize = 20; // Adjust the size as needed

            for (int i = 0; i < field.Height; i++)
            {
                for (int j = 0; j < field.Width; j++)
                {
                    //Label cellLabel = new Label
                    //{
                    //    Text = field.Cells[j, i].symbol.ToString(),
                    //    Location = new Point(j * cellSize, i * cellSize),
                    //    Size = new Size(cellSize, cellSize),
                    //    TextAlign = ContentAlignment.MiddleCenter
                    //};
                    Image cellImage = field.Cells[j, i].img; // Get the image for the symbol

                    PictureBox pictureBox = new PictureBox
                    {
                        Image = cellImage,
                        Location = new Point(j * cellSize, i * cellSize),
                        Size = new Size(cellSize, cellSize),
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    form.Controls.Add(pictureBox);
                    //form.Controls.Add(cellLabel);
                }
            }
        }

        public void DrawEnemies(Field field)
        {
            /*Coordinate currentCursorPosition;
            for (int i = 0; i < field.Height; i++)
            {
                for (int j = 0; j < field.Width; j++)
                {
                    currentCursorPosition = new Coordinate(Console.CursorLeft, Console.CursorTop);
                    Console.SetCursorPosition(j, i);
                    if (field.EnemiesMap[j, i] == null || field.EnemiesMap[j, i].Count == 0)
                    {
                        Console.Write(field.Cells[j, i].symbol);
                    }
                    else
                    {
                        Console.Write(field.EnemiesMap[j, i][field.EnemiesMap[j, i].Count - 1].symbol);
                    }

                    Console.SetCursorPosition(currentCursorPosition.X, currentCursorPosition.Y);
                }
            }*/
        }
        // TODO: метод має приймати лише позицію кожного ворога та перемальвувати тільки його. 

        public void DrawTowers()
        {

        }

        public void DrawMenu(Menu menu)
        {
            form.Controls.Clear();

            int buttonTop = 10;
            foreach (var item in menu.OptionsList)
            {
                Button button = new Button();
                button.Text = item;
                button.Top = buttonTop;
                button.Left = 10;
                button.Width = 200;
                button.Click += (sender, e) => menu.MenuItems[item].Invoke(); 
                form.Controls.Add(button); 

                buttonTop += button.Height + 10;
            }
        }

        public void DrawScore(Field field)
        {
            /*Coordinate currentCursorPosition;
            const int HEIGHT = 8, WIDTH = 30, PADDING = 30;
            for (int i = 0; i < WIDTH; i++)
            {
                currentCursorPosition = new Coordinate(Console.CursorLeft, Console.CursorTop);
                Console.SetCursorPosition(field.Width + i + PADDING, 0);
                Console.Write('░');
                Console.SetCursorPosition(field.Width + i + PADDING, HEIGHT - 1);
                Console.Write('░');
                Console.SetCursorPosition(currentCursorPosition.X, currentCursorPosition.Y);

            }
            for (int i = 1; i < HEIGHT - 1; i++)
            {
                currentCursorPosition = new Coordinate(Console.CursorLeft, Console.CursorTop);
                Console.SetCursorPosition(field.Width + PADDING, i);
                Console.Write("░░");
                Console.SetCursorPosition(field.Width + WIDTH - 2 + PADDING, i);
                Console.Write("░░");
                Console.SetCursorPosition(currentCursorPosition.X, currentCursorPosition.Y);

            }*/
        }
    }

    abstract class Menu
    {
        public Dictionary<string, Action> MenuItems;
        public abstract string[] OptionsList { get; set; }
        //public Dictionary<string, >

        public Menu(in Form _form, Dictionary<string, Action> _menuItems)
        {
            Drawer drawer = new Drawer(_form);
            drawer.DrawMenu(this);
            MenuItems = _menuItems;
        }

        protected int SelectMenuItem()
        {
            return 0;
        }

        protected void Exit()
        {
            Environment.Exit(0);
        }
    }

    class MainMenu : Menu
    {
        private Form form;
        string[] options = new string[] { "New Game", "Exit", };
        public override string[] OptionsList { get => options; set => OptionsList = options; }

        public MainMenu(in Form _form, Dictionary<string, Action> menuItems) : base(_form, menuItems)
        {
            form = _form;
        }
    }

    class SideMenu : Menu
    {
        Form form;
        GameProcess currentGame;
        string[] options = new string[] { "Continue", "New Game", "Exit", };
        public override string[] OptionsList { get => options; set => OptionsList = options; }


        public SideMenu(Form form, Dictionary<string, Action> menuItems, GameProcess game) : base(form, menuItems)// TODO додати Dictioary делегатів
        {
            this.form = form;
            currentGame = game;
        }

    }

    class Field
    {
        public int Width;
        public int Height;
        public Cell[,] Cells = null;
        public Tower[] availableTowers;
        public List<Enemy>[,] EnemiesMap;
        public int EnemyCount = 0;


        public Field(int width, int height)
        {
            availableTowers = new Tower[] {
                new StrongTower(-1, -1, this),
                new RangeTower(-1, -1, this),
                new StrongRangeTower(-1, -1, this)
            };
            Width = width;
            Height = height;
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
                            Cells[j, i] = new EnemyBase(j, i);
                            break;
                        case 'X':
                            Cells[j, i] = new PlayerBase(j, i);
                            break;
                        default:
                            //Console.WriteLine($"Developer is stupid. As always. Ceil ({inputfield[i][j]}) does not exist, so at ({j + 1},{Height - i}) will be wall!");
                            Cells[j, i] = new Wall(j, i);
                            break;
                    }
                }
            }
        }

        public void AddingEnemies(Enemy enemy, int x, int y)
        {
            enemy.MoveEvent += MoveEnemy;
            enemy.EnemyDie += EnemyDie;
            EnemiesMap[x, y].Add(enemy);
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

        public void MoveEnemy(Enemy enemy, int oldX, int oldY, int newX, int newY)
        {
            EnemiesMap[enemy.position.X, enemy.position.Y].Remove(enemy);
            EnemiesMap[newX, newY].Add(enemy);
        }

        private void EnemyDie(object sender, EnemyDieEventArgs e)
        {
            Enemy enemy = e.Enemy;
            int x = enemy.position.X;
            int y = enemy.position.Y;

            EnemiesMap[x, y].Remove(enemy);
        }
    }

    abstract class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public virtual ConsoleColor color { get; set; }
        public virtual bool CanGo { get; set; }
        public virtual char symbol { get; set; }
        public virtual Image img => Properties.Resources.Wall2; //////////


        protected Cell(int posX, int posY)
        {
            X = posX;
            Y = posY;
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


        public EnemyBase(int posX, int posY) : base(posX, posY)
        {

        }

    }

    class PlayerBase : Base
    {
        public override bool CanGo => true;
        public override char symbol => '~';
        public override ConsoleColor color => ConsoleColor.Blue;

        public PlayerBase(int posX, int posY) : base(posX, posY)
        {
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

    abstract class Tower : Cell
    {
        public override bool CanGo => false;
        protected int ATK = 1;
        protected int Range = 1;
        protected int HP = 10;
        public string type { get; set; }
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
    }

    class StrongTower : Tower
    {
        public override char symbol => 'S';


        public StrongTower(int posX, int posY, Field field) : base(posX, posY, field)
        {
            type = "ground";
            ATK = 2;
        }
    }
    class RangeTower : Tower
    {
        public override char symbol => 'R';


        public RangeTower(int posX, int posY, Field field) : base(posX, posY, field)
        {
            type = "wall";
            Range = 2;
        }
    }
    class StrongRangeTower : Tower
    {
        public override char symbol => 'U';


        public StrongRangeTower(int posX, int posY, Field field) : base(posX, posY, field)
        {
            type = "combined";
            ATK = 2;
            Range = 2;
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

    class Enemy
    {
        public delegate void MoveEventHandler(Enemy enemy, int oldX, int oldY, int newX, int newy);
        public event MoveEventHandler MoveEvent;
        public event EventHandler<EnemyDieEventArgs> EnemyDie;
        public double HP { get; set; } = 5;
        public int range = 0;
        public double speed { get; set; } = 1.5;
        public virtual char symbol { get; set; } = 'E';
        public bool IsAlive => HP > 0;
        public Coordinate position { get; set; }
        Cell[,] field;
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


        public Enemy(Cell[,] field)
        {
            this.field = field;
            FindPath();
            moving();
        }

        protected void Fight(Tower target)
        {

        }

        public bool FindPath()
        {
            Coordinate? start = null;
            Coordinate? exit = null;

            foreach (var item in field)
            {
                if (item.GetType() == typeof(EnemyBase))
                {
                    start = new Coordinate(item.X, item.Y);
                    position = start.Value;
                }
                else if (item.GetType() == typeof(PlayerBase))
                {
                    exit = new Coordinate(item.X, item.Y);
                }
            }

            if (start == null || exit == null)
            {
                return false;
            }

            Queue<Coordinate> queue = new Queue<Coordinate>();
            //HashSet<Coordinate?> visited = new HashSet<Coordinate?>();
            Dictionary<Coordinate?, Coordinate?> parent = new Dictionary<Coordinate?, Coordinate?>();

            queue.Enqueue(start.Value);
            //visited.Add(start);
            bool pathFound = false;

            while (queue.Count > 0)
            {
                Coordinate? current = queue.Dequeue();

                if (current.Equals(exit))
                {
                    pathFound = true;
                    break;
                    //TODO: переробити через флаг та break
                }

                foreach (var direction in directions) //+++TODO: бігати одразу по Directions
                {
                    Coordinate? next = new Coordinate(
                        current.Value.X + direction.Value.X,
                        current.Value.Y + direction.Value.Y
                    );

                    if (next != null &&
                        next.Value.X >= 0 && next.Value.X < field.GetLength(0) &&
                        next.Value.Y >= 0 && next.Value.Y < field.GetLength(1))
                    //TODO: винести перевірку меж поля в окрему функцію
                    {
                        Cell nextCell = field[next.Value.X, next.Value.Y];
                        if (!parent.ContainsKey(next) && !next.Equals(start) && nextCell.CanGo)
                        {
                            //visited.Add(next);
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
                /*Console.WriteLine("Found a path from the enemy base to the player base:");
                foreach (var pos in path)
                {
                    Console.Write($"({pos.X}, {pos.Y}) ");
                }*/
                return true;
            }
            //Console.Write("Path doesnt exist");
            return false;
        }

        public async Task moving()
        {
            const int BASE_MOTION_DELAY = 2000;
            foreach (var point in path)
            {
                while (true)
                {
                    await Task.Delay((int)(BASE_MOTION_DELAY / speed));
                    if (field[point.X, point.Y].GetType() == typeof(Space) ||
                        field[point.X, point.Y].GetType() == typeof(EnemyBase))
                    {
                        MoveEvent?.Invoke(this, position.X, position.Y, point.X, point.Y);
                        position = point;

                        break;
                    }
                    else if (field[point.X, point.Y].GetType() == typeof(Tower))
                    {

                    }
                    else if (field[point.X, point.Y].GetType() == typeof(PlayerBase))
                    {
                        OnEnemyDie(new EnemyDieEventArgs(this));
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
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form = new Form1();
            Game newGame = new Game(form); // Передача ссылки на форму в конструктор Game

            form.Show(); // Отображение формы
            Application.Run(form);

            
        }
    }
}
