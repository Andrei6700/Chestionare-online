using Microsoft.AspNetCore.Mvc;
using Chestionare_online.Models;
using Chestionare_online.Data;
using Microsoft.EntityFrameworkCore;

namespace Chestionare_online.Controllers
{
    public class ExamController : Controller
    {
        private readonly AuthDbContext _context;

        public ExamController(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> StartExam()
        {
            var questions = await _context.ExamQuestions 
                .Take(30)
                .ToListAsync();

            if (!questions.Any())
            {
                return View("NoQuestions");
            }

            var firstQuestion = questions.First();
            var viewModel = new QuestionViewModel
            {
                QuestionText = firstQuestion.QuestionText,
                Options = new Dictionary<string, string>
                {
                    {"A", firstQuestion.VariantaA},
                    {"B", firstQuestion.VariantaB},
                    {"C", firstQuestion.VariantaC}
                },
                QuestionNumber = 1,
                TotalQuestions = questions.Count,
                ImageURL = firstQuestion.ImageURL 
            };

            ViewBag.TotalQuestions = viewModel.TotalQuestions;
            ViewBag.RemainingQuestions = viewModel.TotalQuestions - viewModel.QuestionNumber + 1;

            return View("ExamQuestion", viewModel);
        }
    }
}