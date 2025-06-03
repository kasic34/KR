using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiskaBobraFormsApp1
{
    public class Question
    {
        public QuestionType Type { get; set; }
        public object CorrectAnswer { get; set; }

        public Question(QuestionType type, object correctAnswer)
        {
            Type = type;
            CorrectAnswer = correctAnswer;
        }
    }

    public enum QuestionType
    {
        NumericPair,
        Text
    }
}
