﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace rgr_oop
{
    public partial class TicTacToeForm : Form
    {
        private GameLogic game;
        private ComputerAI computerAI;
        private Panel controlPanel;
        private Button btnPlayerVsPlayer;
        private Button btnPlayerVsComputer;
        private Panel gamePanel;
        private Button btnRestart;
        private int cellSize = 30; // Розмір клітинки
        private Panel panel1;
        private Label lblStatus;
        private Panel panel2;

        // Таймер для затримки ходу комп'ютера (режим vs ПК)
        private Timer moveDelayTimer = new Timer();

        public TicTacToeForm()
        {
            InitializeComponent();

            gamePanel.Paint += GamePanel_Paint;
            gamePanel.MouseClick += GamePanel_MouseClick;

            moveDelayTimer.Interval = 500;
            moveDelayTimer.Tick += MoveDelayTimer_Tick;
        }

        private void StartGame(bool vsComputer)
        {
            game = new GameLogic(vsComputer);
            if (vsComputer)
            {
                computerAI = new ComputerAI(game);
                // Комп’ютер починає гру – відображаємо повідомлення про хід пк (хрестик)
                lblStatus.Text = "Хід пк (хрестик)";
                // ПК робить свій хід
                BoardPoint compMove = computerAI.GetComputerMove();
                if (game.MakeMove(compMove, CellState.Cross))
                {
                    if (game.CheckWin(compMove))
                    {
                        lblStatus.Text = "Комп'ютер переміг!";
                        gamePanel.Invalidate();
                        return;
                    }
                }
                // Після ходу комп’ютера хід переходить до гравця (нулик)
                game.CurrentPlayer = CellState.Nought;
                lblStatus.Text = "Хід гравця (нулик)";
            }
            else
            {
                // Для гри два гравці – починає Гравець 1 (хрестик)
                game.CurrentPlayer = CellState.Cross;
                lblStatus.Text = "Гравець 1 (хрестик)";
            }
            gamePanel.Invalidate();
        }

        private void MoveDelayTimer_Tick(object sender, EventArgs e)
        {
            moveDelayTimer.Stop();

            // Хід комп'ютера, який грає як Cross
            BoardPoint compMove = computerAI.GetComputerMove();
            if (game.MakeMove(compMove, CellState.Cross))
            {
                gamePanel.Invalidate();
                if (game.CheckWin(compMove))
                {
                    lblStatus.Text = "Комп'ютер переміг!";
                    return;
                }
            }
            // Після ходу комп'ютера – хід гравця (нулик)
            game.CurrentPlayer = CellState.Nought;
            lblStatus.Text = "Хід гравця (нулик)";
        }

        private void GamePanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (game == null)
                return;

            // У режимі з ПК дозволяємо клік лише коли хід гравця (нулик)
            if (game.IsPlayerVsComputer && game.CurrentPlayer != CellState.Nought)
            {
                Debug.WriteLine("Очікуємо хід комп'ютера.");
                return;
            }

            int offsetX = 0;
            int offsetY = 0;
            int col = (e.X - offsetX) / cellSize;
            int row = (e.Y - offsetY) / cellSize;
            BoardPoint move = new BoardPoint(col, row);
            Debug.WriteLine($"Хід гравця: {move}");

            if (game.MakeMove(move, game.CurrentPlayer))
            {
                gamePanel.Invalidate();
                if (game.CheckWin(move))
                {
                    // При виграші повідомляємо про перемогу відповідного учасника
                    if (game.IsPlayerVsComputer)
                        lblStatus.Text = "Гравець (нулик) переміг!";
                    else
                        lblStatus.Text = $"Гравець {(game.CurrentPlayer == CellState.Cross ? "1 (хрестик)" : "2 (нулик)")} переміг!";
                    Debug.WriteLine("Перемога.");
                    return;
                }

                if (game.IsPlayerVsComputer)
                {
                    // Після ходу гравця в режимі vs ПК запускаємо таймер для ходу комп'ютера.
                    // Перед запуском встановлюємо CurrentPlayer для комп'ютера (хрестик) і показуємо повідомлення
                    game.CurrentPlayer = CellState.Cross;
                    lblStatus.Text = "Хід пк (хрестик)";
                    moveDelayTimer.Start();
                }
                else
                {
                    // Для гри два гравці – перемикаємо чергу і повідомляємо відповідно
                    game.CurrentPlayer = (game.CurrentPlayer == CellState.Cross) ? CellState.Nought : CellState.Cross;
                    lblStatus.Text = (game.CurrentPlayer == CellState.Cross)
                        ? "Гравець 1 (хрестик)"
                        : "Гравець 2 (нулик)";
                }
            }
            else
            {
                Debug.WriteLine("Неможливий хід — клітинка зайнята або не ваша черга.");
            }
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (game == null || game.Board == null)
                return;

            Graphics g = e.Graphics;
            g.Clear(Color.White);

            int offsetX = 0;
            int offsetY = 0;
            int columns = gamePanel.Width / cellSize;
            int rows = gamePanel.Height / cellSize;

            // Малюємо сітку
            for (int i = 0; i <= columns; i++)
            {
                int x = offsetX + i * cellSize;
                g.DrawLine(Pens.LightGray, x, offsetY, x, offsetY + rows * cellSize);
            }
            for (int j = 0; j <= rows; j++)
            {
                int y = offsetY + j * cellSize;
                g.DrawLine(Pens.LightGray, offsetX, y, offsetX + columns * cellSize, y);
            }

            // Малюємо ходи
            foreach (var kvp in game.Board)
            {
                BoardPoint p = kvp.Key;
                int x = offsetX + p.X * cellSize;
                int y = offsetY + p.Y * cellSize;
                Rectangle rect = new Rectangle(x, y, cellSize, cellSize);
                if (kvp.Value == CellState.Cross)
                {
                    g.DrawLine(Pens.Red, rect.Left, rect.Top, rect.Right, rect.Bottom);
                    g.DrawLine(Pens.Red, rect.Right, rect.Top, rect.Left, rect.Bottom);
                }
                else if (kvp.Value == CellState.Nought)
                {
                    g.DrawEllipse(Pens.Blue, rect);
                }
            }
        }

        // Обробники кнопок
        private void btnPlayerVsComputer_Click(object sender, EventArgs e)
        {
            StartGame(true);
        }

        private void btnPlayerVsPlayer_Click(object sender, EventArgs e)
        {
            StartGame(false);
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            StartGame(game.IsPlayerVsComputer);
        }

        private void InitializeComponent()
        {
            // (Оригінальний код ініціалізації форми без змін)
            this.controlPanel = new System.Windows.Forms.Panel();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnPlayerVsPlayer = new System.Windows.Forms.Button();
            this.btnPlayerVsComputer = new System.Windows.Forms.Button();
            this.gamePanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.controlPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlPanel
            // 
            this.controlPanel.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.controlPanel.Controls.Add(this.btnPlayerVsPlayer);
            this.controlPanel.Controls.Add(this.btnPlayerVsComputer);
            this.controlPanel.Location = new System.Drawing.Point(0, 420);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(400, 80);
            this.controlPanel.TabIndex = 3;
            // 
            // btnRestart
            // 
            this.btnRestart.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btnRestart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestart.Font = new System.Drawing.Font("Calibri", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRestart.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnRestart.Location = new System.Drawing.Point(6, 0);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(200, 80);
            this.btnRestart.TabIndex = 3;
            this.btnRestart.Text = "Перезапуск";
            this.btnRestart.UseVisualStyleBackColor = false;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnPlayerVsPlayer
            // 
            this.btnPlayerVsPlayer.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btnPlayerVsPlayer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlayerVsPlayer.Font = new System.Drawing.Font("Calibri", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPlayerVsPlayer.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnPlayerVsPlayer.Location = new System.Drawing.Point(200, 0);
            this.btnPlayerVsPlayer.Name = "btnPlayerVsPlayer";
            this.btnPlayerVsPlayer.Size = new System.Drawing.Size(200, 80);
            this.btnPlayerVsPlayer.TabIndex = 2;
            this.btnPlayerVsPlayer.Text = "Два гравці";
            this.btnPlayerVsPlayer.UseVisualStyleBackColor = false;
            this.btnPlayerVsPlayer.Click += new System.EventHandler(this.btnPlayerVsPlayer_Click);
            // 
            // btnPlayerVsComputer
            // 
            this.btnPlayerVsComputer.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btnPlayerVsComputer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlayerVsComputer.Font = new System.Drawing.Font("Calibri", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnPlayerVsComputer.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnPlayerVsComputer.Location = new System.Drawing.Point(0, 0);
            this.btnPlayerVsComputer.Margin = new System.Windows.Forms.Padding(0);
            this.btnPlayerVsComputer.Name = "btnPlayerVsComputer";
            this.btnPlayerVsComputer.Size = new System.Drawing.Size(200, 80);
            this.btnPlayerVsComputer.TabIndex = 1;
            this.btnPlayerVsComputer.Text = "Гра з ПК";
            this.btnPlayerVsComputer.UseVisualStyleBackColor = false;
            this.btnPlayerVsComputer.Click += new System.EventHandler(this.btnPlayerVsComputer_Click);
            // 
            // gamePanel
            // 
            this.gamePanel.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.gamePanel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.gamePanel.Location = new System.Drawing.Point(80, 110);
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(900, 300);
            this.gamePanel.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Location = new System.Drawing.Point(0, 20);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(900, 80);
            this.panel1.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Calibri", 35F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblStatus.Location = new System.Drawing.Point(80, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(546, 72);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Виберіть режим гри";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.panel2.Controls.Add(this.btnRestart);
            this.panel2.Location = new System.Drawing.Point(771, 420);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(209, 80);
            this.panel2.TabIndex = 4;
            // 
            // TicTacToeForm
            // 
            this.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.ClientSize = new System.Drawing.Size(982, 553);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.gamePanel);
            this.Name = "TicTacToeForm";
            this.Load += new System.EventHandler(this.TicTacToeForm_Load);
            this.controlPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void TicTacToeForm_Load(object sender, EventArgs e)
        {
            // Початкове завантаження. За бажанням можна автоматично стартувати гру.
        }
    }
}
