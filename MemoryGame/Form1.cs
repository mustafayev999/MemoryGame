namespace MemoryGame;

public partial class Form1 : Form
{
    private readonly Color[] cardColors =
    {
        Color.Crimson, Color.Crimson,
        Color.DodgerBlue, Color.DodgerBlue,
        Color.Gold, Color.Gold,
        Color.MediumSeaGreen, Color.MediumSeaGreen
    };

    private readonly Button[] cards = new Button[8];
    private readonly bool[] matched = new bool[8];
    private readonly Color[] currentColors = new Color[8];
    private readonly System.Windows.Forms.Timer hideTimer = new();
    private readonly Random random = new();

    private int firstCard = -1;
    private int secondCard = -1;
    private int correctSteps;
    private int wrongSteps;
    private bool waitingForTimer;

    private readonly Label correctLabel = new();
    private readonly Label wrongLabel = new();
    private readonly Label resultLabel = new();
    private readonly Button newGameButton = new();
    private readonly Button exitButton = new();

    public Form1()
    {
        BuildInterface();
        hideTimer.Interval = 850;
        hideTimer.Tick += HideTimer_Tick;
        StartNewGame();
    }

    private void BuildInterface()
    {
        Text = "Memory Game";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 550);
        MinimumSize = new Size(800, 480);
        BackColor = Color.Black;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10, FontStyle.Bold);

        var board = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 390,
            ColumnCount = 4,
            RowCount = 2,
            BackColor = Color.Black,
            Padding = new Padding(35, 20, 35, 0)
        };

        for (int i = 0; i < 4; i++)
            board.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (int i = 0; i < 2; i++)
            board.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        for (int i = 0; i < cards.Length; i++)
        {
            var card = new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(18, 12, 18, 12),
                BackColor = Color.Silver,
                FlatStyle = FlatStyle.Flat,
                Text = "",
                Tag = i,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            card.FlatAppearance.BorderColor = Color.Gainsboro;
            card.FlatAppearance.BorderSize = 2;
            card.Click += Card_Click;
            cards[i] = card;
            board.Controls.Add(card, i % 4, i / 4);
        }

        Controls.Add(board);

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = Color.Black,
            Padding = new Padding(75, 5, 65, 15)
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        correctLabel.Text = "Düzgün addım sayı: 0";
        wrongLabel.Text = "Səhv addım sayı: 0";
        foreach (var label in new[] { correctLabel, wrongLabel })
        {
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.ForeColor = Color.White;
            label.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        resultLabel.Text = "";
        resultLabel.Dock = DockStyle.Fill;
        resultLabel.TextAlign = ContentAlignment.MiddleCenter;
        resultLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);

        StyleActionButton(newGameButton, "Yeni oyun", Color.Lime);
        StyleActionButton(exitButton, "Çıxış", Color.Red);
        newGameButton.Click += (_, _) => StartNewGame();
        exitButton.Click += ExitButton_Click;

        bottom.Controls.Add(correctLabel, 0, 0);
        bottom.Controls.Add(resultLabel, 1, 0);
        bottom.Controls.Add(newGameButton, 1, 1);
        bottom.Controls.Add(wrongLabel, 0, 1);
        bottom.Controls.Add(exitButton, 2, 1);
        Controls.Add(bottom);
    }

    private static void StyleActionButton(Button button, string text, Color borderColor)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(18, 8, 18, 8);
        button.BackColor = Color.Black;
        button.ForeColor = borderColor;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.BorderSize = 1;
        button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    private void StartNewGame()
    {
        hideTimer.Stop();
        firstCard = -1;
        secondCard = -1;
        waitingForTimer = false;
        correctSteps = 0;
        wrongSteps = 0;

        // Hər rəngdən iki kart olmaqla kartların sırasını qarışdırırıq.
        var shuffled = cardColors.OrderBy(_ => random.Next()).ToArray();

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].BackColor = Color.Silver;
            cards[i].Text = "";
            cards[i].Enabled = true;
            cards[i].FlatAppearance.BorderColor = Color.Gainsboro;
            matched[i] = false;
            currentColors[i] = shuffled[i];
            cards[i].Tag = i;
        }

        UpdateScore();
        resultLabel.Text = "";
    }

    private void Card_Click(object? sender, EventArgs e)
    {
        if (waitingForTimer || sender is not Button clicked)
            return;

        int index = (int)clicked.Tag!;

        if (matched[index] || index == firstCard)
            return;

        clicked.BackColor = currentColors[index];
        clicked.Text = " ";
        clicked.FlatAppearance.BorderColor = Color.DeepSkyBlue;

        if (firstCard == -1)
        {
            firstCard = index;
            return;
        }

        secondCard = index;
        CheckPair();
    }

    private void CheckPair()
    {
        Color firstColor = currentColors[firstCard];
        Color secondColor = currentColors[secondCard];

        if (firstColor == secondColor)
        {
            matched[firstCard] = true;
            matched[secondCard] = true;
            correctSteps++;
            resultLabel.ForeColor = Color.Lime;
            resultLabel.Text = "✅";
            cards[firstCard].Text = "✅";
            cards[secondCard].Text = "✅";
            cards[firstCard].Enabled = false;
            cards[secondCard].Enabled = false;
            firstCard = -1;
            secondCard = -1;
            UpdateScore();

            if (matched.All(value => value))
            {
                resultLabel.Text = "✅  Oyun bitdi!";
                MessageBox.Show(
                    $"Təbriklər! Bütün rəng cütlərini tapdınız.\nDüzgün addım sayı: {correctSteps}\nSəhv addım sayı: {wrongSteps}",
                    "Oyun bitdi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else
        {
            wrongSteps++;
            resultLabel.ForeColor = Color.Red;
            resultLabel.Text = "❌";
            cards[firstCard].Text = "❌";
            cards[secondCard].Text = "❌";
            UpdateScore();
            waitingForTimer = true;
            hideTimer.Start();
        }
    }

    private void HideTimer_Tick(object? sender, EventArgs e)
    {
        hideTimer.Stop();

        foreach (int index in new[] { firstCard, secondCard })
        {
            cards[index].BackColor = Color.Silver;
            cards[index].Text = "";
            cards[index].FlatAppearance.BorderColor = Color.Gainsboro;
        }

        firstCard = -1;
        secondCard = -1;
        waitingForTimer = false;
        resultLabel.Text = "";
    }

    private void UpdateScore()
    {
        correctLabel.Text = $"Düzgün addım sayı: {correctSteps}";
        wrongLabel.Text = $"Səhv addım sayı: {wrongSteps}";
    }

    private void ExitButton_Click(object? sender, EventArgs e)
    {
        DialogResult answer = MessageBox.Show(
            "Proqramdan çıxış edilsinmi?",
            "Bildiriş",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
            Close();
    }
}

