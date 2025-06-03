using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PiskaBobraFormsApp1
{
    public partial class Form2 : Form
    {
        public string ReturnedData { get; set; }
        int currentRound = 1;
        int score = 0;
        List<object> userAnswers = new List<object>();
        Font radioDefaultFont;

        enum QuestionType { NumericPair, Text }

        class Question
        {
            public QuestionType Type { get; }
            public object CorrectAnswer { get; }
            public List<string> Options { get; }

            public Question(QuestionType type, object correctAnswer, List<string> options)
            {
                Type = type;
                CorrectAnswer = correctAnswer;
                Options = options;
            }
        }

        List<Question> questions = new List<Question>()
        {
            new Question(QuestionType.NumericPair, new Tuple<double, double>(2.0, -3.0), new List<string> { "2 и -3", "3 и -2", "1 и -4" }),
            new Question(QuestionType.NumericPair, new Tuple<double, double>(1.0, 1.0), new List<string> { "1 и 1", "2 и 0", "0.5 и 1.5" }),
            new Question(QuestionType.Text, "Пифагор", new List<string> { "Пифагор", "Архимед", "Евклид" }),
            new Question(QuestionType.NumericPair, new Tuple<double, double>(4.0, -2.0), new List<string> { "4 и -2", "-4 и 2", "3 и -1" })
        };

        void LoadRound(int round)
        {
            radioButton1.Visible = radioButton2.Visible = radioButton3.Visible = false;

            if (round > questions.Count)
            {
                button1.Enabled = false;
                pictureBox1.Image = null;
                SaveResults();
                return;
            }

            pictureBox1.Image = Image.FromFile($"task{round}.jpg");
            ResetRadioStyles();

            var question = questions[round - 1];
            this.Text = question.Type == QuestionType.NumericPair ?
                "Режим конторольной работы" :
                "Выберите правильный ответ:";

            radioButton1.Text = question.Options[0];
            radioButton2.Text = question.Options[1];
            radioButton3.Text = question.Options[2];

            radioButton1.Visible = radioButton2.Visible = radioButton3.Visible = true;
            button1.Text = "Ответить";
        }

        void ResetRadioStyles()
        {
            radioButton1.ForeColor = SystemColors.ControlText;
            radioButton2.ForeColor = SystemColors.ControlText;
            radioButton3.ForeColor = SystemColors.ControlText;
            radioButton1.Font = radioButton2.Font = radioButton3.Font = radioDefaultFont;
        }

        bool CheckAnswer(int round, int selectedOptionIndex)
        {
            var question = questions[round - 1];
            string selectedText = GetRadioText(selectedOptionIndex);

            if (question.Type == QuestionType.NumericPair)
            {
                var correctPair = (Tuple<double, double>)question.CorrectAnswer;
                string correctText1 = $"{correctPair.Item1} и {correctPair.Item2}";
                string correctText2 = $"{correctPair.Item2} и {correctPair.Item1}";

                return selectedText == correctText1 || selectedText == correctText2;
            }
            else if (question.Type == QuestionType.Text)
            {
                return selectedText.Equals(question.CorrectAnswer.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        string GetRadioText(int index)
        {
            switch (index)
            {
                case 0: return radioButton1.Text;
                case 1: return radioButton2.Text;
                case 2: return radioButton3.Text;
                default: return "";
            }
        }

        private void SaveResults()
        {
            string fileName = $"Результаты_конторольной_работы_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            int grade = CalculateGrade(score, questions.Count);

            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    sw.WriteLine($"Результаты контрольной работы от {DateTime.Now}");
                    sw.WriteLine($"Правильных ответов: {score} из {questions.Count}");
                    sw.WriteLine($"Оценка: {grade}");
                    sw.WriteLine("=".PadRight(50, '='));

                    for (int i = 0; i < questions.Count; i++)
                    {
                        sw.WriteLine($"\nВопрос #{i + 1}");
                        sw.WriteLine($"Тип вопроса: {questions[i].Type}");
                        sw.WriteLine($"Ваш ответ: {userAnswers[i]}");
                        sw.WriteLine($"Правильный ответ: {FormatAnswer(questions[i].CorrectAnswer)}");
                        sw.WriteLine($"Результат: {(IsAnswerCorrect(i) ? "Правильно" : "Неправильно")}");
                    }
                }

                this.ReturnedData = $"Оценка: {grade}. Правильных ответов: {score} из {questions.Count}";
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatAnswer(object answer)
        {
            if (answer is Tuple<double, double> doublePair)
            {
                return $"{doublePair.Item1} и {doublePair.Item2}";
            }
            return answer.ToString();
        }

        private bool IsAnswerCorrect(int questionIndex)
        {
            var question = questions[questionIndex];
            var userAnswer = userAnswers[questionIndex].ToString();
            string correctAnswer = FormatAnswer(question.CorrectAnswer);

            return userAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyAnswerStyles(bool isCorrect, int selectedIndex)
        {
            ResetRadioStyles();
            int correctIndex = -1;

            var question = questions[currentRound - 1];
            for (int i = 0; i < 3; i++)
            {
                if (GetRadioText(i) == FormatAnswer(question.CorrectAnswer))
                {
                    correctIndex = i;
                    break;
                }
            }

            if (isCorrect)
            {
                SetRadioStyle(selectedIndex, Color.Green, FontStyle.Bold);
            }
            else
            {
                SetRadioStyle(selectedIndex, Color.Red, FontStyle.Italic);
                SetRadioStyle(correctIndex, Color.Green, FontStyle.Bold);
            }
        }

        private void SetRadioStyle(int index, Color color, FontStyle style)
        {
            RadioButton rb = null;
            switch (index)
            {
                case 0: rb = radioButton1; break;
                case 1: rb = radioButton2; break;
                case 2: rb = radioButton3; break;
            }

            if (rb != null)
            {
                rb.ForeColor = color;
                rb.Font = new Font(rb.Font, style);
            }
        }

        private int CalculateGrade(int correctAnswers, int totalQuestions)
        {
            if (correctAnswers == totalQuestions) return 5;
            if (correctAnswers == totalQuestions - 1) return 4;
            if (correctAnswers >= totalQuestions / 2) return 3;
            return 2;
        }

        public Form2()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            radioDefaultFont = radioButton1.Font;
            LoadRound(currentRound);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Далее")
            {
                currentRound++;
                LoadRound(currentRound);
                return;
            }
            else if (button1.Text == "Завершить")
            {
                SaveResults(); // Сохраняем результаты и закрываем форму
                return;
            }

            int selectedIndex = -1;
            if (radioButton1.Checked) selectedIndex = 0;
            else if (radioButton2.Checked) selectedIndex = 1;
            else if (radioButton3.Checked) selectedIndex = 2;

            if (selectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите вариант ответа", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedText = GetRadioText(selectedIndex);
            bool isCorrect = CheckAnswer(currentRound, selectedIndex);
            userAnswers.Add(selectedText);

            if (isCorrect) score++;
            ApplyAnswerStyles(isCorrect, selectedIndex);

            if (isCorrect)
            {
                MessageBox.Show("Верно!", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var question = questions[currentRound - 1];
                MessageBox.Show($"Неверно. Правильный ответ: {FormatAnswer(question.CorrectAnswer)}", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (currentRound == questions.Count)
            {
                button1.Text = "Завершить";
            }
            else
            {
                button1.Text = "Далее";
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }
    }
}