using Microsoft.AspNetCore.Mvc;
using Chestionare_online.Models;
using Chestionare_online.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Chestionare_online.Controllers
{
    public class ExamController : Controller
    {
        private readonly AuthDbContext _context;

        public ExamController(AuthDbContext context)
        {
            _context = context;
        }

// function used to start the exam
        public async Task<IActionResult> StartExam()
        {
            // select 26 question, random form data base
            var questionIds = await _context.ExamQuestions
                .Select(q => q.Id)
                .OrderBy(x => Guid.NewGuid())
                .Take(26)
                .ToListAsync();

            HttpContext.Session.SetString("ExamQuestions", JsonSerializer.Serialize(questionIds));
            HttpContext.Session.SetInt32("CurrentQuestionIndex", 0);
            HttpContext.Session.SetInt32("CorrectAnswers", 0);
            HttpContext.Session.SetInt32("WrongAnswers", 0);

            // asve the timer when the exam started
            HttpContext.Session.SetString("ExamStartTime", DateTime.UtcNow.ToString("o"));

            return RedirectToAction("ShowQuestion");
        }

// function that show the question
        public async Task<IActionResult> ShowQuestion()
        {
            // get the question from the database
            var questionIds = JsonSerializer.Deserialize<List<int>>(
                HttpContext.Session.GetString("ExamQuestions")) ?? new List<int>();

            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;

// if the exam is finished, redirect to the result page
            if (currentIndex >= questionIds.Count)
            {
                return RedirectToAction("ExamResult");
            }

            var question = await _context.ExamQuestions
                .FirstOrDefaultAsync(q => q.Id == questionIds[currentIndex]);

            // remove the question id , i don t need it
            question.QuestionText = Regex.Replace(question.QuestionText, @"^\d+\.\s?", "");

            // read the time when the exam started
            var startTimeString = HttpContext.Session.GetString("ExamStartTime");

    // if the time is not valid, set the current time
            DateTime startTime;
            if (!DateTime.TryParse(startTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out startTime))
            {
                startTime = DateTime.UtcNow; 
            }

            // time in seconds
            int examDuration = 30 * 60; // 30 min in sec
            int elapsedSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            int remainingTime = Math.Max(0, examDuration - elapsedSeconds);

            ViewBag.RemainingTime = remainingTime;

// if the time is over, redirect to the fail page
            var viewModel = new QuestionViewModel
            {
                QuestionText = question.QuestionText,
                Options = new Dictionary<string, string>
        {
            {"A", question.VariantaA},
            {"B", question.VariantaB},
            {"C", question.VariantaC}
        },
                QuestionNumber = currentIndex + 1,
                TotalQuestions = questionIds.Count,
                ImageURL = question.ImageURL
            };

            ViewBag.TotalQuestions = viewModel.TotalQuestions;
            ViewBag.RemainingQuestions = viewModel.TotalQuestions - viewModel.QuestionNumber;

            return View("ExamQuestion", viewModel);
        }


        [HttpPost]
        // send the answer to db, then redirect to the next question or to the result page
        public async Task<IActionResult> SubmitAnswer(string selectedAnswer)
        {
            var questionIds = JsonSerializer.Deserialize<List<int>>(
                HttpContext.Session.GetString("ExamQuestions")) ?? new List<int>();

            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;

            // check the wrong answers
            var wrongAnswers = HttpContext.Session.GetInt32("WrongAnswers") ?? 0;
            if (wrongAnswers >= 4)
            {
                return RedirectToAction("FailExam", new { reason = "mistakes" });
            }

            // chec the good answers 
            var currentQuestionId = questionIds[currentIndex];
            var correctAnswer = await _context.ExamQuestions
                .Where(q => q.Id == currentQuestionId)
                .Select(q => q.CorrectAnswer)
                .FirstOrDefaultAsync();

            // update the correct and wrong answers 
                        if (selectedAnswer == correctAnswer)
            {
                HttpContext.Session.SetInt32("CorrectAnswers",
                    (HttpContext.Session.GetInt32("CorrectAnswers") ?? 0) + 1);
            }
            else
            {
                HttpContext.Session.SetInt32("WrongAnswers",
                    (HttpContext.Session.GetInt32("WrongAnswers") ?? 0) + 1);
            }

            // nex question
            HttpContext.Session.SetInt32("CurrentQuestionIndex", currentIndex + 1);
            return RedirectToAction("ShowQuestion");


        }
// function that show the result of the exam
        public IActionResult ExamResult()
        {
            var correct = HttpContext.Session.GetInt32("CorrectAnswers") ?? 0;
            var wrong = HttpContext.Session.GetInt32("WrongAnswers") ?? 0;

            ViewBag.Total = correct + wrong;
            ViewBag.Correct = correct;
            ViewBag.Wrong = wrong;

            return View();
        }
        // redirect to the fail page
        public IActionResult FailExam(string reason)
        {
            // clean the session
            HttpContext.Session.Remove("ExamQuestions");
            HttpContext.Session.Remove("CurrentQuestionIndex");
            HttpContext.Session.Remove("CorrectAnswers");
            HttpContext.Session.Remove("WrongAnswers");

            // clean the cache for the timer
            ViewBag.ClearTimer = true;

            // mesage
            ViewBag.FailReason = reason switch
            {
                "timeout" => "Ați depășit timpul alocat!",
                "mistakes" => "Ați acumulat prea multe greșeli!",
                _ => "Ați fost respins la examen!"
            };

            return View();
        }
    }
}