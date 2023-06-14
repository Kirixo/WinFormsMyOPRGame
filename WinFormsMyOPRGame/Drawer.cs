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
using System.Drawing.Imaging;

namespace MyOPRGame
{
    class Drawer
    {
        public class nonFocusedButton : Button
        {
            public nonFocusedButton()
            {
                SetStyle(ControlStyles.Selectable, false);
            }
        }
        public Form form;
        public int cellSize = 20;
        private PictureBox[,] imgMatrix;
        public nonFocusedButton[] towerButtons;
        //private PictureBox[,] imgMatrix2;
        //private static Semaphore semaphore = new Semaphore(1, 1);


        public Drawer(Form form)
        {
            this.form = form;
        }

        public void InitMatrix(Field field)
        {
            imgMatrix = new PictureBox[field.Width, field.Height];
            for (int i = 0; i < field.Height; i++)
            {
                for (int j = 0; j < field.Width; j++)
                {
                    imgMatrix[j, i] = new PictureBox();
                    imgMatrix[j, i].Location = new Point(j * cellSize, i * cellSize);
                    imgMatrix[j, i].Size = new Size(cellSize, cellSize);
                    imgMatrix[j, i].SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        public void DrawField(Field field)
        {
            InitMatrix(field);
            form.Controls.Clear();
            for (int i = 0; i < field.Height; i++)
            {
                for (int j = 0; j < field.Width; j++)
                {
                    form.Controls.Add(imgMatrix[j, i]);
                }
            }

            int top = 0, bot = field.Height - 1, left = 0, right = field.Width - 1;
            while(top <= bot && left <= right)
            {
                for(int i = left; i <= right; i++)
                {
                    DrawPic(new Coordinate(i, top), field.Cells[i, top].img);
                }
                top++;
                for (int i = top; i <= bot; i++)
                {
                    DrawPic(new Coordinate(right, i), field.Cells[right, i].img);
                }
                right--;
                for (int i = right; i >= left; i--)
                {
                    DrawPic(new Coordinate(i, bot), field.Cells[i, bot].img);
                }
                bot--;
                for (int i = bot; i >= top; i--)
                {
                    DrawPic(new Coordinate(left, i), field.Cells[left, i].img);
                }
                left++;
            }
        }

        public void DrawPic(Coordinate coordinate, Image cellImage)
        {
            int x = coordinate.X;
            int y = coordinate.Y;
            imgMatrix[x, y].Image = cellImage;
            imgMatrix[x, y].Invalidate();
            form.Invoke(new Action(() =>
            {
                imgMatrix[x, y].Update();
            }));
        }

        public void DrawTowerSelectBox(Field field)
        {
            const int TAB = 20;
            for (int i = 0; i < field.availableTowers.Length; i++)
            {
                towerButtons[i].Location = new Point((TAB + 60) * i + TAB, field.Height * cellSize + TAB);
                towerButtons[i].Size = new Size(60, 60);
                towerButtons[i].Image = field.availableTowers[i].img.GetThumbnailImage(towerButtons[i].Width - 10, towerButtons[i].Height - 10, null, IntPtr.Zero); ;

                form.Controls.Add(towerButtons[i]);

                //buttonTop += button.Height + 10;
            }
        }

        public Image tempImg(Image img)
        {
            Image originalImage = img;

            // Создаем новый Bitmap для измененной картинки
            Bitmap adjustedImage = new Bitmap(originalImage.Width, originalImage.Height);

            // Создаем объект Graphics для рисования на новой картинке
            using (Graphics graphics = Graphics.FromImage(adjustedImage))
            {
                // Создаем объект ImageAttributes для изменения оттенка
                ImageAttributes imageAttributes = new ImageAttributes();

                // Устанавливаем матрицу цветовых преобразований, чтобы изменить оттенок
                ColorMatrix colorMatrix = new ColorMatrix();
                colorMatrix.Matrix33 = 0.5f; // Здесь можно задать желаемое значение яркости (от 0.0 до 1.0)

                // Устанавливаем матрицу цветовых преобразований в объекте ImageAttributes
                imageAttributes.SetColorMatrix(colorMatrix);

                // Рисуем измененную картинку с примененным оттенком
                graphics.DrawImage(originalImage, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height),
                    0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            // Отображаем измененную картинку в PictureBox
            return adjustedImage;
        }

        public void DrawEnemie(Field field, Coordinate was, Coordinate now)
        {
            Image cellImage = field.Cells[was.X, was.Y].img;
            DrawPic(new Coordinate(was.X, was.Y), cellImage);
            cellImage = field.EnemiesMap[now.X, now.Y][field.EnemiesMap[now.X, now.Y].Count - 1].img;
            DrawPic(new Coordinate(now.X, now.Y), cellImage);
        }

        public void DrawCeil(Field field, Coordinate coord)
        {
            if (field.EnemiesMap[coord.X, coord.Y] == null || field.EnemiesMap[coord.X, coord.Y].Count == 0)
            {
                Image cellImage = field.Cells[coord.X, coord.Y].img;
                DrawPic(new Coordinate(coord.X, coord.Y), cellImage);
            }
            else
            {
                Image cellImage2 = field.EnemiesMap[coord.X, coord.Y][field.EnemiesMap[coord.X, coord.Y].Count - 1].img;
                DrawPic(new Coordinate(coord.X, coord.Y), cellImage2);
            }
        }

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
                button.Click += (sender, e) => menu.OptionChecker(item);
                form.Controls.Add(button);

                buttonTop += button.Height + 10;
            }
        }

        public void DrawLost()
        {
            form.Invoke(new Action(() =>
            {
                form.Controls.Clear();
            }));
            PictureBox picture = new PictureBox
            {
                Size = new Size(form.Width, form.Height),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Image gifImage = Properties.Resources.LoseScreen;
            picture.Image = gifImage;
            form.Invoke(new Action(() =>
            {
                form.Controls.Add(picture);
                //ImageAnimator.Animate(gifImage, (sender, e) =>
                //{
                //    picture.Invalidate();
                //});
            }));
        }

        public void DrawWin()
        {
            form.Invoke(new Action(() =>
            {
                form.Controls.Clear();
            }));
            PictureBox picture = new PictureBox
            {
                Size = new Size(form.Width, form.Height),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Image gifImage = Properties.Resources.WinScreen;
            picture.Image = gifImage;
            form.Invoke(new Action(() =>
            {
                form.Controls.Add(picture);
                ImageAnimator.Animate(gifImage, (sender, e) =>
                {
                    picture.Invalidate();
                });
            }));
        }

        public void DrawScore(Field field)
        {
        }
    }
}
