using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chestionare_online.Models
{
    // class used to store a question in the database
    public class ExamQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("intrebare", TypeName = "text")]
        public string QuestionText { get; set; }

        [Column("varianta_a")]
        public string VariantaA { get; set; }

        [Column("varianta_b")]
        public string VariantaB { get; set; }

        [Column("varianta_c")]
        public string VariantaC { get; set; }

        [Required]
        [Column("CorrectAnswer")]
        public string CorrectAnswer { get; set; }

        [Column("ImageURL")]
        public string? ImageURL { get; set; }
    }
}