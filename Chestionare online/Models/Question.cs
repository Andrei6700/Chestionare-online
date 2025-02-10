namespace Chestionare_online.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public string CorrectAnswer { get; set; }
    }
}