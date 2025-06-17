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

        // Добавлены PictureBox для вариантов ответов
        PictureBox[] answerPictureBoxes;

        enum QuestionType { NumericPair, Text }

        class Question
        {
            public QuestionType Type { get; }
            public object CorrectAnswer { get; }
            public List<string> Options { get; }
            public int StartImageIndex { get; } // Стартовый индекс для картинок ответов

            public Question(QuestionType type, object correctAnswer, List<string> options, int startImageIndex)
            {
                Type = type;
                CorrectAnswer = correctAnswer;
                Options = options;
                StartImageIndex = startImageIndex;
            }
        }

        List<Question> questions = new List<Question>()
        {
            new Question(QuestionType.NumericPair, new Tuple<double, double>(1, 2),
                new List<string> { "1", "2", "3", "4" }, 1),
            new Question(QuestionType.NumericPair, new Tuple<double, double>(3, 1),
                new List<string> { "1", "2", "3", "4" }, 5),
            new Question(QuestionType.Text, "1",
                new List<string> { "1", "2", "3", "4" }, 9),
            new Question(QuestionType.NumericPair, new Tuple<double, double>(4, 1),
                new List<string> { "1", "2", "3", "4" }, 13)
        };

        public Form2()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            radioDefaultFont = radioButton1.Font;

            // Инициализация массива PictureBox для ответов
            answerPictureBoxes = new PictureBox[]
            {
                pictureBox2, pictureBox3, pictureBox4, pictureBox5
            };

            LoadRound(currentRound);
        }

        void LoadRound(int round)
        {
            // Скрываем все элементы управления
            radioButton1.Visible = radioButton2.Visible = radioButton3.Visible = radioButton4.Visible = false;
            pictureBox2.Visible = pictureBox3.Visible = pictureBox4.Visible = pictureBox5.Visible = false;

            if (round > questions.Count)
            {
                button1.Visible = false;
                button3.Enabled = false;
                pictureBox1.Image = null;
                SaveResults();
                return;
            }

            // Загрузка основного изображения вопроса
            try
            {
                pictureBox1.Image = Image.FromFile($"Question{round}.jpg");
            }
            catch
            {
                MessageBox.Show($"Файл изображения Question{round}.jpg не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ResetRadioStyles();

            var question = questions[round - 1];
            this.Text = question.Type == QuestionType.NumericPair ?
                "Режим контрольной работы" :
                "Выберите правильный ответ:";

            // Устанавливаем текст и изображения для вариантов ответов
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        radioButton1.Text = question.Options[i];
                        radioButton1.Visible = true;
                        break;
                    case 1:
                        radioButton2.Text = question.Options[i];
                        radioButton2.Visible = true;
                        break;
                    case 2:
                        radioButton3.Text = question.Options[i];
                        radioButton3.Visible = true;
                        break;
                    case 3:
                        radioButton4.Text = question.Options[i];
                        radioButton4.Visible = true;
                        break;
                }

                // Загрузка изображений для вариантов ответов
                try
                {
                    // Формируем имя файла: Answer + (стартовый индекс + номер варианта)
                    string imagePath = $"Answer{question.StartImageIndex + i}.jpg";
                    answerPictureBoxes[i].Image = Image.FromFile(imagePath);
                    answerPictureBoxes[i].Visible = true;
                }
                catch
                {
                    // Обработка отсутствия изображения
                    answerPictureBoxes[i].Image = null;
                }
            }

            // Восстановление предыдущего ответа
            if (userAnswers.Count >= round)
            {
                string prevAnswer = userAnswers[round - 1].ToString();
                if (radioButton1.Text == prevAnswer) radioButton1.Checked = true;
                else if (radioButton2.Text == prevAnswer) radioButton2.Checked = true;
                else if (radioButton3.Text == prevAnswer) radioButton3.Checked = true;
                else if (radioButton4.Text == prevAnswer) radioButton4.Checked = true;

                button1.Visible = false;
            }
            else
            {
                radioButton1.Checked = radioButton2.Checked = radioButton3.Checked = radioButton4.Checked = false;
                button1.Visible = true;
            }

            // Управление кнопками навигации
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

        private int CalculateGrade(int correctAnswers, int totalQuestions)
        {
            if (correctAnswers == totalQuestions) return 5;
            if (correctAnswers == totalQuestions - 1) return 4;
            if (correctAnswers >= totalQuestions / 2) return 3;
            return 2;
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

            button1.Visible = false;
            button3.Enabled = currentRound < questions.Count;
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

        private void button5_Click(object sender, EventArgs e)
        {
            Hint hintForm = new Hint();
            hintForm.Show();
        }

        // Освобождаем ресурсы изображений при закрытии формы
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            foreach (var pb in answerPictureBoxes)
            {
                if (pb.Image != null)
                {
                    pb.Image.Dispose();
                }
            }
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }
    }
}