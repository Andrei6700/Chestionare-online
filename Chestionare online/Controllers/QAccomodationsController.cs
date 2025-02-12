using Microsoft.AspNetCore.Mvc;
using Chestionare_online.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Chestionare_online.Controllers
{
    public class QAccomodationsController : Controller
    {
        // List of sample questions
        private List<Question> _questions = new List<Question>
        {
            new Question { Id = 1, Text = "1 + 1 = ?", Options = new Dictionary<string, string> { {"A", "2"}, {"B", "10"}, {"C", "15"}, {"D", "50"}, {"E", "100"} }, CorrectAnswer = "A" },
            new Question { Id = 2, Text = "Cate minute sunt intr-o ora?", Options = new Dictionary<string, string> { {"A", "10"}, {"B", "20"}, {"C", "30"}, {"D", "60"}, {"E", "100"} }, CorrectAnswer = "D" },
            new Question { Id = 3, Text = "Care din urmatoarele alimente sunt fructe?", Options = new Dictionary<string, string> { {"A", "Mar"}, {"B", "Piersica"}, {"C", "Lapte"}, {"D", "Ou"}, {"E", "Pruna"} }, CorrectAnswer = "A,B,E" }
        };

        // Main action for handling questions
        public IActionResult PageQAccomodations(
            string category,
            string questionQueue,
            int? orderNumber,
            Dictionary<string, string> answers,
            string currentAnswer,
            bool skip = false)
        {
            // Initialize answers dictionary
            if (answers == null) answers = new Dictionary<string, string>();

            // Handle question queue
            List<int> queue;
            if (string.IsNullOrEmpty(questionQueue))
            {
                // First time setup
                queue = Enumerable.Range(0, _questions.Count).ToList();
                orderNumber = 1;
            }
            else
            {
                // Convert queue from string to list
                try { queue = questionQueue.Split(',').Select(int.Parse).ToList(); }
                catch { queue = Enumerable.Range(0, _questions.Count).ToList(); orderNumber = 1; }

                // Set order number if missing
                if (!orderNumber.HasValue) orderNumber = answers.Count + 1;
            }

            // Check if all questions answered
            if (!queue.Any())
            {
                var score = CalculateScore(answers);
                return View("Result", new { CorrectAnswers = score, TotalQuestions = _questions.Count });
            }

            // Get current question
            int currentQuestionIndex = queue[0];

            // Handle answer submission
            if (!skip && !string.IsNullOrEmpty(currentAnswer))
            {
                answers[currentQuestionIndex.ToString()] = currentAnswer;
                queue.RemoveAt(0);
                orderNumber++;
            }
            else if (skip)
            {
                // Move question to end of queue
                queue.RemoveAt(0);
                queue.Add(currentQuestionIndex);
            }

            // Check again if queue empty after processing
            if (!queue.Any())
            {
                var score = CalculateScore(answers);
                return View("Result", new { CorrectAnswers = score, TotalQuestions = _questions.Count });
            }

            //  next question
            var nextQuestion = _questions[queue[0]];
            string newQueue = string.Join(",", queue);

            return View("Question", new
            {
                Question = nextQuestion,
                OrderNumber = orderNumber,
                TotalQuestions = _questions.Count,
                Answers = answers,
                QuestionQueue = newQueue,
                Category = category
            });
        }

        //  method to calculate score
        private int CalculateScore(Dictionary<string, string> answers)
        {
            int score = 0;
            for (int i = 0; i < _questions.Count; i++)
            {
                if (answers.TryGetValue(i.ToString(), out string userAnswer))
                {
                    var correct = _questions[i].CorrectAnswer.Split(',').Select(x => x.Trim()).OrderBy(x => x);
                    var user = userAnswer.Split(',').Select(x => x.Trim()).OrderBy(x => x);
                    if (correct.SequenceEqual(user)) score++;
                }
            }
            return score;
        }
    }
}