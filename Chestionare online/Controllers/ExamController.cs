using Microsoft.AspNetCore.Mvc;
using Chestionare_online.Models;
using System.Collections.Generic;

namespace Chestionare_online.Controllers
{
    public class ExamController : Controller
    {
        private static List<Question> _examQuestions = new List<Question> // just for testing. I want to do first frontend 
        {
            new Question
            {
                Id = 1,
                Text = "În care dintre următoarele situații conducătorilor de vehicule le este interzis să execute virajul spre stânga la lumina de culoare verde a semaforului electric?",
                Options = new Dictionary<string, string>
                {
                    {"A", "când întâlnesc indicatorul «Înainte sau la stânga»"},
                    {"B", "când se află pe o stradă cu sens unic"},
                    {"C", "când întâlnesc indicatorul «Înainte sau la dreapta»"}
                },
                CorrectAnswer = "C"
            }
            
        };

        public IActionResult StartExam()
        {
            // create a view model
            var viewModel = new QuestionViewModel 
            {
                QuestionText = _examQuestions[0].Text,
                Options = _examQuestions[0].Options,
                QuestionNumber = 1,
                TotalQuestions = _examQuestions.Count,
                CorrectAnswers = 0,
                WrongAnswers = 0
            };

            // send the view model to the view
            ViewBag.TotalQuestions = viewModel.TotalQuestions;
            ViewBag.RemainingQuestions = viewModel.TotalQuestions - viewModel.QuestionNumber + 1;
            ViewBag.CorrectAnswers = viewModel.CorrectAnswers;
            ViewBag.WrongAnswers = viewModel.WrongAnswers;

            return View("ExamQuestion", viewModel);
        }
    }
}