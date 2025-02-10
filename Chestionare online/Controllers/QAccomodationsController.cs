using Microsoft.AspNetCore.Mvc;
using Chestionare_online.Models;
using System.Collections.Generic;
using System.Linq;

namespace Chestionare_online.Controllers
{
    // controller for the questions
    public class QAccomodationsController : Controller
    {
        private List<Question> _questions = new List<Question>
        {
 // list of the questions with correct answers
            new Question
            {
                Id = 1,
                Text = "1 + 1 = ?",
                Options = new Dictionary<string, string>
                {
                    {"A", "2"}, {"B", "10"}, {"C", "15"}, {"D", "50"}, {"E", "100"}
                },
                CorrectAnswer = "A"
            },
            new Question
            {
                Id = 2,
                Text = "Câte minute sunt într-o oră?",
                Options = new Dictionary<string, string>
                {
                    {"A", "10"}, {"B", "20"}, {"C", "30"}, {"D", "60"}, {"E", "100"}
                },
                CorrectAnswer = "D"
            },
            new Question
            {
                Id = 3,
                Text = "Care din următoarele alimente sunt fructe?",
                Options = new Dictionary<string, string>
                {
                    {"A", "Măr"}, {"B", "Piersică"}, {"C", "Lapte"}, {"D", "Ou"}, {"E", "Prună"}
                },
                CorrectAnswer = "A,B,E"
            }
        };

        public IActionResult PageQAccomodations(string category, int? questionId, List<string> answers, List<int> skippedQuestions)
        {
            ViewBag.SelectedCategory = category;
            skippedQuestions ??= new List<int>();

            // initialize firs questiom
            if (questionId == null)
            {
                return View("Question", new
                {
                    Question = _questions[0],
                    QuestionNumber = 1,
                    TotalQuestions = _questions.Count,
                    Answers = new List<string>(),
                    SkippedQuestions = new List<int>() // list of skipped questions
                });
            }
            // get the current question
            int currentIndex = (int)questionId - 1;

            if (Request.Query.ContainsKey("skip"))
            {
                if (!skippedQuestions.Contains(currentIndex))
                {
                    skippedQuestions.Add(currentIndex);
                }
                currentIndex++; 
            }

            if (currentIndex >= _questions.Count)
            {
                if (skippedQuestions.Any())
                {
                    int nextQuestionIndex = skippedQuestions[0];
                    skippedQuestions.RemoveAt(0);

                    return View("Question", new
                    {
                        Question = _questions[nextQuestionIndex],
                        QuestionNumber = nextQuestionIndex + 1,
                        TotalQuestions = _questions.Count,
                        Answers = answers,
                        SkippedQuestions = skippedQuestions
                    });
                }
                else
                {
                    int correctAnswers = CalculateScore(answers);
                    return View("Result", new
                    {
                        CorrectAnswers = correctAnswers,
                        TotalQuestions = _questions.Count
                    });
                }
            }

            return View("Question", new
            {
                Question = _questions[currentIndex],
                QuestionNumber = currentIndex + 1,
                TotalQuestions = _questions.Count,
                Answers = answers,
                SkippedQuestions = skippedQuestions
            });
        }

        private int CalculateScore(List<string> answers)
        {
            if (answers == null || answers.Count != _questions.Count) return 0;

            int score = 0;
            for (int i = 0; i < _questions.Count; i++)
            {
                var correctAnswers = _questions[i].CorrectAnswer.Split(',');
                var userAnswers = answers[i].Split(',');

                bool isPerfectMatch = correctAnswers.Length == userAnswers.Length
                                   && correctAnswers.All(ca => userAnswers.Contains(ca)); // check if the answers are correct

                if (isPerfectMatch)
                {
                    score++;
                }
            }
            return score;
        }
    }
}
