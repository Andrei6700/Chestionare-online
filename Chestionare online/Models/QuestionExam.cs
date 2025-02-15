using System.Collections.Generic;

namespace Chestionare_online.Models
{
    public class QuestionViewModel 
    {
        public string QuestionText { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public int QuestionNumber { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public string? ImageURL { get; internal set; }
    }
}