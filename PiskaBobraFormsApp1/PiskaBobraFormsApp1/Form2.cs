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
            new Question(QuestionType.NumericPair, new Tuple<double, double>(2.0, -3.0), new List<string> { "2 и -3", "3 и -2", "1 и -4", "-1 и 1" }),
            new Question(QuestionType.NumericPair, new Tuple<double, double>(1.0, -4.0), new List<string> { "2 и -3", "3 и -2", "1 и -4", "-1 и 1" }),
            new Question(QuestionType.Text, "Архимед", new List<string> { "Пифагор", "Архимед", "Евклид", "Декарт" }),
            new Question(QuestionType.NumericPair, new Tuple<double, double>(-1.0, 1.0), new List<string> { "4 и -2", "-4 и 2", "3 и -1", "-1 и 1" })
        };

        void LoadRound(int round)
        {
            // Скрываем кнопку "Ответить" если все вопросы отвечены
            button1.Visible = userAnswers.Count < questions.Count;

            // Скрываем все radioButton'ы перед загрузкой нового вопроса
            radioButton1.Visible = radioButton2.Visible = radioButton3.Visible = radioButton4.Visible = false;

            if (round > questions.Count)
            {
                button1.Visible = false;
                button3.Enabled = false;
                pictureBox1.Image = null;
                SaveResults();
                return;
            }

            pictureBox1.Image = Image.FromFile($"task{round}.jpg");
            ResetRadioStyles();

            var question = questions[round - 1];
            this.Text = question.Type == QuestionType.NumericPair ?
                "Режим контрольной работы" :
                "Выберите правильный ответ:";

            // Устанавливаем текст для radioButton'ов
            radioButton1.Text = question.Options[0];
            radioButton2.Text = question.Options[1];
            radioButton3.Text = question.Options[2];
            radioButton4.Text = question.Options.Count > 3 ? question.Options[3] : "";

            // Показываем только нужные radioButton'ы
            radioButton1.Visible = true;
            radioButton2.Visible = true;
            radioButton3.Visible = true;
            radioButton4.Visible = question.Options.Count > 3;

            // Устанавливаем выбранный вариант, если ответ уже был дан
            if (userAnswers.Count >= round)
            {
                string prevAnswer = userAnswers[round - 1].ToString();
                if (radioButton1.Text == prevAnswer) radioButton1.Checked = true;
                else if (radioButton2.Text == prevAnswer) radioButton2.Checked = true;
                else if (radioButton3.Text == prevAnswer) radioButton3.Checked = true;
                else if (radioButton4.Visible && radioButton4.Text == prevAnswer) radioButton4.Checked = true;
            }
            else
            {
                radioButton1.Checked = radioButton2.Checked = radioButton3.Checked = radioButton4.Checked = false;
            }

            // Управление доступностью кнопок "Назад" и "Вперёд"
            button4.Enabled = currentRound > 1;
            button3.Enabled = currentRound < questions.Count;
        }

        void ResetRadioStyles()
        {
            radioButton1.ForeColor = SystemColors.ControlText;
            radioButton2.ForeColor = SystemColors.ControlText;
            radioButton3.ForeColor = SystemColors.ControlText;
            radioButton4.ForeColor = SystemColors.ControlText;
            radioButton1.Font = radioButton2.Font = radioButton3.Font = radioButton4.Font = radioDefaultFont;
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
                case 3: return radioButton4.Text;
                default: return "";
            }
        }

        private void SaveResults()
        {
            string fileName = $"Результаты_контрольной_работы_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
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

                        if (i < userAnswers.Count)
                        {
                            sw.WriteLine($"Ваш ответ: {userAnswers[i]}");
                            sw.WriteLine($"Результат: {(IsAnswerCorrect(i) ? "Правильно" : "Неправильно")}");
                        }
                        else
                        {
                            sw.WriteLine("Ваш ответ: (не был дан)");
                            sw.WriteLine("Результат: Неправильно");
                        }

                        sw.WriteLine($"Правильный ответ: {FormatAnswer(questions[i].CorrectAnswer)}");
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

        private void SetRadioStyle(int index, Color color, FontStyle style)
        {
            RadioButton rb = null;
            switch (index)
            {
                case 0: rb = radioButton1; break;
                case 1: rb = radioButton2; break;
                case 2: rb = radioButton3; break;
                case 3: rb = radioButton4; break;
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
            int selectedIndex = -1;
            if (radioButton1.Checked) selectedIndex = 0;
            else if (radioButton2.Checked) selectedIndex = 1;
            else if (radioButton3.Checked) selectedIndex = 2;
            else if (radioButton4.Checked) selectedIndex = 3;

            if (selectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите вариант ответа", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedText = GetRadioText(selectedIndex);
            bool isCorrect = CheckAnswer(currentRound, selectedIndex);

            if (userAnswers.Count >= currentRound)
                userAnswers[currentRound - 1] = selectedText;
            else
                userAnswers.Add(selectedText);

            if (isCorrect) score++;

            // Скрываем кнопку "Ответить" если ответили на все вопросы
            button1.Visible = userAnswers.Count < questions.Count;

            if (currentRound < questions.Count)
            {
                currentRound++;
                LoadRound(currentRound);
            }
            else
            {
                button3.Enabled = false;
                SaveResults();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currentRound > 1)
            {
                currentRound--;
                LoadRound(currentRound);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentRound < questions.Count)
            {
                currentRound++;
                LoadRound(currentRound);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (userAnswers.Count < questions.Count)
            {
                var result = MessageBox.Show("Вы ответили не на все вопросы. Завершить тест досрочно?",
                                             "Подтверждение",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    SaveResults();
                }
            }
            else
            {
                SaveResults();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}