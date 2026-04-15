using System.ComponentModel.DataAnnotations;

namespace PlayMaker.ViewsModel
{
    public class CommentViewModel
    {
        [Required(ErrorMessage = "Yorum metni boş olamaz.")]
        [Display(Name = "Your Comment")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Yorum türü seçilmelidir.")]
        [Display(Name = "Comment Type")]
        public string CommentType { get; set; }

        [Required(ErrorMessage = "Yorum hedefi seçilmelidir.")]
        [Display(Name = "Select Target")]
        public string CommentTargetGol { get; set; }
    }
}
