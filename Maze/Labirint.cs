using System;
using System.Windows.Forms;
using System.Drawing;
using System.Media;
using System.Collections.Generic;

namespace Maze
{
    class Labirint
    {
        public int height; // высота лабиринта (количество строк)
        public int width; // ширина лабиринта (количество столбцов в каждой строке)

        private int playerX;
        private int playerY;

        private int health;

        private int collectedMedals;
        private int totalMedals = 0;

        public MazeObject[,] maze;
        public PictureBox[,] images;

        public static Random r = new Random();
        public Form parent;

        private int energy;
        private int movesAfterMedicine;

        private SoundPlayer backgroundMusicPlayer;
        private SoundPlayer explosionSoundPlayer;
        private SoundPlayer kickSoundPlayer;

        private int lastMoveX;
        private int lastMoveY;
        private int bulletX;
        private int bulletY;

        private bool bombPlaced;
        private int bombX;
        private int bombY;

        private int moveCount;

        private List<Point> enemyPositions;
        private List<Point> enemyDirections;

        private int numberOfEnemies;
        private Point[] enemyPositions3;
        private Point[] enemyDirections3;

        public Labirint(Form parent, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.parent = parent;

            maze = new MazeObject[height, width];
            images = new PictureBox[height, width];

            Generate();

            playerX = 0;
            playerY = 2;

            health = 100;

            energy = 500;
            movesAfterMedicine = 0;

            backgroundMusicPlayer = new SoundPlayer("background_music.mp3");
            explosionSoundPlayer = new SoundPlayer("explosion_sound.mp3");
            kickSoundPlayer = new SoundPlayer("kick_sound.mp3");

            moveCount = 0;

            enemyPositions = new List<Point>();
            enemyDirections = new List<Point>();

            this.numberOfEnemies = 0;
        }

        private void Generate()
        {
            int smileX = 0;
            int smileY = 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    MazeObject.MazeObjectType current = MazeObject.MazeObjectType.HALL;

                    // в 1 случае из 5 - ставим стену
                    if (r.Next(5) == 0)
                    {
                        current = MazeObject.MazeObjectType.WALL;
                    }

                    // в 1 случае из 250 - кладём денежку
                    if (r.Next(250) == 0)
                    {
                        current = MazeObject.MazeObjectType.MEDAL;
                        totalMedals++;
                    }

                    // в 1 случае из 250 - размещаем врага
                    if (r.Next(250) == 0)
                    {
                        current = MazeObject.MazeObjectType.ENEMY;
                        numberOfEnemies++;
                    }

                    // стены по периметру обязательны
                    if (y == 0 || x == 0 || y == height - 1 | x == width - 1)
                    {
                        current = MazeObject.MazeObjectType.WALL;
                    }

                    // наш персонажик
                    if (x == smileX && y == smileY)
                    {
                        current = MazeObject.MazeObjectType.CHAR;
                    }

                    // лекарство
                    if (r.Next(100) == 0)
                    {
                        current = MazeObject.MazeObjectType.MEDICINE;
                    }

                    // кофе
                    if (r.Next(250) == 0)
                    {
                        current = MazeObject.MazeObjectType.COFFEE;
                    }


                    // есть выход, и соседняя ячейка справа всегда свободна
                    if (x == smileX + 1 && y == smileY || x == width - 1 && y == height - 3)
                    {
                        current = MazeObject.MazeObjectType.HALL;
                    }

                    maze[y, x] = new MazeObject(current);
                    images[y, x] = new PictureBox();
                    images[y, x].Location = new Point(x * maze[y, x].width, y * maze[y, x].height);
                    images[y, x].Parent = parent;
                    images[y, x].Width = maze[y, x].width;
                    images[y, x].Height = maze[y, x].height;
                    images[y, x].BackgroundImage = maze[y, x].texture;
                    images[y, x].Visible = false;

                    if (current == MazeObject.MazeObjectType.ENEMY)
                    {
                        this.enemyPositions.Add(new Point(x * maze[y, x].width, y * maze[y, x].height));
                    }
                }
            }
            enemyPositions3 = new Point[numberOfEnemies];
            enemyDirections3 = new Point[numberOfEnemies];
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if(maze[y, x].type == MazeObject.MazeObjectType.ENEMY)
                    {
                        enemyPositions3[i] = new Point(x * maze[y, x].width, y * maze[y, x].height);
                        i++;
                    }
                }
            }
        }

        public void Show()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    images[y, x].Visible = true;
                }
            }

            PlayBackgroundMusic();
        }

        public void MovePlayer(Keys key)
        {
            int newX = playerX;
            int newY = playerY;

            switch (key)
            {
                case Keys.Left:
                    newX--;
                    lastMoveX = -1;
                    lastMoveY = 0;
                    break;
                case Keys.Right:
                    newX++;
                    lastMoveX = 1;
                    lastMoveY = 0;
                    break;
                case Keys.Up:
                    newY--;
                    lastMoveX = 0;
                    lastMoveY = -1;
                    break;
                case Keys.Down:
                    newY++;
                    lastMoveX = 0;
                    lastMoveY = 1;
                    break;
            }

            if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX].type != MazeObject.MazeObjectType.WALL)
            {
                images[playerY, playerX].BackgroundImage = new MazeObject(MazeObject.MazeObjectType.HALL).texture;

                playerX = newX;
                playerY = newY;

                images[playerY, playerX].BackgroundImage = new MazeObject(MazeObject.MazeObjectType.CHAR).texture;

                if (playerX == width - 1 && health > 0)
                {
                    MessageBox.Show("Победа - найден выход!");
                    parent.Close();
                }
            }

            if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX].type != MazeObject.MazeObjectType.WALL)
            {
                playerX = newX;
                playerY = newY;

                if (maze[playerY, playerX].type == MazeObject.MazeObjectType.MEDAL)
                {
                    maze[playerY, playerX].type = MazeObject.MazeObjectType.HALL;
                    collectedMedals++;
                    ShowCollectedMedals();
                }
                else if (maze[playerY, playerX].type == MazeObject.MazeObjectType.ENEMY)
                {
                    maze[playerY, playerX].type = MazeObject.MazeObjectType.HALL;
                    int damage = r.Next(20, 26);
                    health -= damage;

                    if (health <= 0)
                    {
                        MessageBox.Show("Поражение - закончилось здоровье!");
                        parent.Close();
                    }
                }
                else if (maze[playerY, playerX].type == MazeObject.MazeObjectType.MEDICINE)
                {

                    if (health < 100)
                    {
                        health += 5;
                        maze[playerY, playerX].type = MazeObject.MazeObjectType.HALL;
                        if (health > 100)
                        {
                            health = 100;
                        }
                    }
                    ShowCollectedMedals();
                }
                else if (maze[playerY, playerX].type == MazeObject.MazeObjectType.COFFEE)
                {
                    if (movesAfterMedicine >= 10)
                    {
                        energy += 25;
                        maze[playerY, playerX].type = MazeObject.MazeObjectType.HALL;
                        ShowCollectedMedals();
                    }
                }
                else
                {
                    energy--;
                    movesAfterMedicine++;
                }

                moveCount++;

                if (moveCount % 10 == 0)
                {
                    SpawnEnemy();
                }
            }

            if (collectedMedals == totalMedals && health > 0)
            {
                MessageBox.Show("Победа - все медали собраны!");
                parent.Close();
            }

            if (energy <= 0)
            {
                MessageBox.Show("Поражение - закончилась энергия!");
                parent.Close();
            }

            if (NoMoreEnemies())
            {
                MessageBox.Show("Победа - враги уничтожены!");
                parent.Close();
            }

            if (key == Keys.Shift && energy >= 10)
            {
                Attack();
                return;
            }

            if (key == Keys.Tab && energy >= 10)
            {
                AttackWithBlaster();
            }

            if (key == Keys.Enter)
            {
                PlaceBomb();
            }
            else if (key == Keys.Space)
            {
                DetonateBomb();
            }

            MoveEnemies();
        }

        public void ShowCollectedMedals()
        {
            parent.Text = "Собрано медалей: " + collectedMedals + " | Здоровье: " + health + "% | Энергия: " + energy;
        }

        private void Attack()
        {
            if (energy >= 10)
            {
                energy -= 10;

                for (int y = playerY - 1; y <= playerY + 1; y++)
                {
                    for (int x = playerX - 1; x <= playerX + 1; x++)
                    {
                        if (x >= 0 && x < width && y >= 0 && y < height && maze[y, x].type == MazeObject.MazeObjectType.ENEMY)
                        {
                            maze[y, x].type = MazeObject.MazeObjectType.HALL;
                        }
                    }
                }
            }
        }

        private void SpawnEnemy()
        {
            int spawnX = r.Next(1, width - 1); 
            int spawnY = r.Next(1, height - 1); 

            if (maze[spawnY, spawnX].type == MazeObject.MazeObjectType.HALL)
            {
                maze[spawnY, spawnX].type = MazeObject.MazeObjectType.ENEMY;
            }
        }
        private bool NoMoreEnemies()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (maze[y, x].type == MazeObject.MazeObjectType.ENEMY)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void PlayBackgroundMusic()
        {
            backgroundMusicPlayer.PlayLooping();
        }

        private void PlayExplosionSound()
        {
            explosionSoundPlayer.Play();
        }

        private void PlayKickSound()
        {
            kickSoundPlayer.Play();
        }

        private void AttackWithBlaster()
        {
            if (energy >= 20)
            {
                int bulletStartX = playerX;
                int bulletStartY = playerY;
                bulletX = playerX + lastMoveX;
                bulletY = playerY + lastMoveY;

                while (bulletX >= 0 && bulletX < width && bulletY >= 0 && bulletY < height)
                {
                    if (maze[bulletY, bulletX].type == MazeObject.MazeObjectType.WALL)
                    {
                        break; // Рикошет от стены
                    }
                    else if (maze[bulletY, bulletX].type == MazeObject.MazeObjectType.ENEMY)
                    {
                        maze[bulletY, bulletX].type = MazeObject.MazeObjectType.HALL;
                        break;
                    }

                    bulletX += lastMoveX;
                    bulletY += lastMoveY;
                }

                energy -= 20;
            }
        }

        private void PlaceBomb()
        {
            if (!bombPlaced && energy >= 49)
            {
                bombX = playerX;
                bombY = playerY;
                bombPlaced = true;
                energy -= 49;
            }
        }

        private void DetonateBomb()
        {
            if (bombPlaced && energy >= 1)
            {
                for (int y = Math.Max(0, bombY - 3); y <= Math.Min(height - 1, bombY + 3); y++)
                {
                    for (int x = Math.Max(0, bombX - 3); x <= Math.Min(width - 1, bombX + 3); x++)
                    {
                        maze[y, x].type = MazeObject.MazeObjectType.HALL; // Уничтожение объектов в радиусе взрыва
                    }
                }

                bombPlaced = false;
                energy -= 1;
            }
        }

        private void MoveEnemies()
        {
            List<Point> enemyPositions = new List<Point>();

            // Создание списка текущих позиций врагов
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (maze[y, x].type == MazeObject.MazeObjectType.ENEMY)
                    {
                        enemyPositions.Add(new Point(x, y));
                    }
                }
            }

            // Перемещение каждого врага
            foreach (Point enemyPos in enemyPositions)
            {
                int currentX = enemyPos.X;
                int currentY = enemyPos.Y;

                List<Point> availableMoves = GetAvailableMoves(currentX, currentY);

                if (availableMoves.Count > 0)
                {
                    int randomIndex = r.Next(availableMoves.Count);
                    Point newEnemyPos = availableMoves[randomIndex];

                    maze[currentY, currentX].type = MazeObject.MazeObjectType.HALL;
                    maze[newEnemyPos.Y, newEnemyPos.X].type = MazeObject.MazeObjectType.ENEMY;
                }
            }
        }

        private List<Point> GetAvailableMoves(int x, int y)
        {
            List<Point> availableMoves = new List<Point>();

            // Проверка доступности каждого соседнего направления
            Point[] directions = {
        new Point(-1, 0),   // Влево
        new Point(1, 0),    // Вправо
        new Point(0, -1),   // Вверх
        new Point(0, 1)     // Вниз
    };

            foreach (Point direction in directions)
            {
                int newX = x + direction.X;
                int newY = y + direction.Y;

                if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newY, newX].type == MazeObject.MazeObjectType.HALL)
                {
                    availableMoves.Add(new Point(newX, newY));
                }
            }

            return availableMoves;
        }

        private void MoveDirectEnemies()
        {
            for (int i = 0; i < enemyPositions.Count; i++)
            {
                Point currentPos = enemyPositions[i];
                Point currentDir = enemyDirections[i];

                int newX = currentPos.X + currentDir.X;
                int newY = currentPos.Y + currentDir.Y;

                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    MazeObject.MazeObjectType obstacleType = maze[newY, newX].type;

                    if (obstacleType != MazeObject.MazeObjectType.WALL && obstacleType != MazeObject.MazeObjectType.ENEMY)
                    {
                        maze[currentPos.Y, currentPos.X].type = MazeObject.MazeObjectType.HALL;
                        maze[newY, newX].type = MazeObject.MazeObjectType.ENEMY;

                        if (obstacleType == MazeObject.MazeObjectType.MEDAL && obstacleType == MazeObject.MazeObjectType.COFFEE && obstacleType == MazeObject.MazeObjectType.MEDICINE)
                        {
                            // Враг подбирает медали и другие объекты, кроме стен
                            maze[newY, newX] = new MazeObject(MazeObject.MazeObjectType.HALL);
                            // Увеличайте счетчик подобранных объектов для врага, если необходимо
                        }

                        enemyPositions[i] = new Point(newX, newY);
                        continue;
                    }
                }

                // Обновляем направление врага при столкновении с препятствием
                Point newDir = GenerateRandomDirection();
                enemyDirections[i] = newDir;
            }
        }

        private Point GenerateRandomDirection()
        {
            Point[] directions = {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1)};

            int randomIndex = r.Next(0, directions.Length);
            return directions[randomIndex];
        }
        private bool IsWithinBounds(Point position)
        {
            return position.X >= 0 && position.X < maze.GetLength(0) && position.Y >= 0 && position.Y < maze.GetLength(1);
        }

        private bool IsCellFree(Point position)
        {
            return maze[position.X, position.Y].type != MazeObject.MazeObjectType.ENEMY;
        }

        public void MoveExtremEnemies()
        {
            for (int i = 0; i < numberOfEnemies; i++)
            {
                Point nextPosition = new Point(enemyPositions3[i].X + 2 * enemyDirections3[i].X, enemyPositions3[i].Y + 2 * enemyDirections3[i].Y);

                if (IsWithinBounds(nextPosition) && IsCellFree(nextPosition))
                {
                    enemyPositions[i] = nextPosition;
                }
                else
                {
                    enemyDirections3[i] = GenerateRandomDirection();
                }
            }
        }
    }
}
