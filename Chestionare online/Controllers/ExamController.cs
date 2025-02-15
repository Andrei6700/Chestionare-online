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

            HttpContext.Session.SetString("ExamQuestionQueue", JsonSerializer.Serialize(questionIds));
            HttpContext.Session.SetInt32("CorrectAnswers", 0);
            HttpContext.Session.SetInt32("WrongAnswers", 0);
            HttpContext.Session.SetString("ExamStartTime", DateTime.UtcNow.ToString("o"));             // asve the timer when the exam started

            return RedirectToAction("ShowQuestion");
        }
        // function that show the question
        public async Task<IActionResult> ShowQuestion()
        {
            // check if we have question in the queue
            var questionQueue = JsonSerializer.Deserialize<List<int>>(
                HttpContext.Session.GetString("ExamQuestionQueue")) ?? new List<int>();

            if (questionQueue.Count == 0)
            {
                return RedirectToAction("ExamResult");
            }

            // get the first question from the queue
            var currentQuestionId = questionQueue[0];
            var question = await _context.ExamQuestions
                .FirstOrDefaultAsync(q => q.Id == currentQuestionId);            // get the question from the database

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
            int examDuration = 30 * 60;
            int elapsedSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            int remainingTime = Math.Max(0, examDuration - elapsedSeconds);

            // if the time is over, redirect to the fail page
            if (remainingTime <= 0)
            {
                return RedirectToAction("FailExam", new { reason = "timeout" });
            }

            ViewBag.RemainingTime = remainingTime;

            // Prepare view model
            var viewModel = new QuestionViewModel
            {
                QuestionText = question.QuestionText,
                Options = new Dictionary<string, string>
                {
                    {"A", question.VariantaA},
                    {"B", question.VariantaB},
                    {"C", question.VariantaC}
                },
                QuestionNumber = 26 - questionQueue.Count + 1,
                TotalQuestions = 26,
                ImageURL = question.ImageURL
            };

            ViewBag.TotalQuestions = 26;
            ViewBag.RemainingQuestions = questionQueue.Count - 1;

            return View("ExamQuestion", viewModel);
        }

        // function that submit the answer
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(string selectedAnswers)
        {
            // get the question from the queue
            var questionQueue = JsonSerializer.Deserialize<List<int>>(
                HttpContext.Session.GetString("ExamQuestionQueue")) ?? new List<int>();

            if (questionQueue.Count == 0) return RedirectToAction("ExamResult");

            var currentQuestionId = questionQueue[0];
            var question = await _context.ExamQuestions
                .FirstOrDefaultAsync(q => q.Id == currentQuestionId);

            // check the answer
            var userAnswers = (selectedAnswers ?? "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim().ToUpper())
                .OrderBy(a => a)
                .ToList();

            // get the correct answer
            var correctAnswers = question.CorrectAnswer
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim().ToUpper())
                .OrderBy(a => a)
                .ToList();

            bool isCorrect = userAnswers.Count == correctAnswers.Count &&
                           userAnswers.SequenceEqual(correctAnswers);

            // up the correct or wrong answers
            if (isCorrect)
            {
                HttpContext.Session.SetInt32("CorrectAnswers",
                    (HttpContext.Session.GetInt32("CorrectAnswers") ?? 0) + 1);
            }
            else
            {
                HttpContext.Session.SetInt32("WrongAnswers",
                    (HttpContext.Session.GetInt32("WrongAnswers") ?? 0) + 1);
            }

            // remove the question from the queue
            questionQueue.RemoveAt(0);
            HttpContext.Session.SetString("ExamQuestionQueue", JsonSerializer.Serialize(questionQueue));

            return RedirectToAction("ShowQuestion");
        }

        [HttpPost]
        //  skip the question
        public IActionResult SkipQuestion()
        {
            var questionQueue = JsonSerializer.Deserialize<List<int>>(
                HttpContext.Session.GetString("ExamQuestionQueue")) ?? new List<int>();

            if (questionQueue.Count == 0)
            {
                return RedirectToAction("ExamResult");
            }

            // remove the question from the queue and add it at the end
            var currentQuestionId = questionQueue[0];
            questionQueue.RemoveAt(0);
            questionQueue.Add(currentQuestionId);
            HttpContext.Session.SetString("ExamQuestionQueue", JsonSerializer.Serialize(questionQueue));

            return RedirectToAction("ShowQuestion");
        }
        //  show the result of the exam
        public IActionResult ExamResult()
        {
            var correct = HttpContext.Session.GetInt32("CorrectAnswers") ?? 0;
            var wrong = HttpContext.Session.GetInt32("WrongAnswers") ?? 0;

            ViewBag.Total = correct + wrong;
            ViewBag.Correct = correct;
            ViewBag.Wrong = wrong;

            // clear the session
            HttpContext.Session.Remove("ExamQuestionQueue");
            HttpContext.Session.Remove("CorrectAnswers");
            HttpContext.Session.Remove("WrongAnswers");

            return View();
        }

        // redirect to the fail page
        public IActionResult FailExam(string reason)
        {
            // clean the session
            HttpContext.Session.Remove("ExamQuestionQueue");
            HttpContext.Session.Remove("CorrectAnswers");
            HttpContext.Session.Remove("WrongAnswers");

            ViewBag.ClearTimer = true; // clean the cache for the timer
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