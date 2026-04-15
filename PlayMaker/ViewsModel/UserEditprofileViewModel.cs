namespace PlayMaker.ViewsModel
{
    public class UserEditprofileViewModel
    {
      
            public string UserName { get; set; }

            public IFormFile? ProfileImage { get; set; }

            public string? ExistingProfilePhotoPath { get; set; }

            public string? CurrentPassword { get; set; }

            public string? NewPassword { get; set; }

            public string? ConfirmPassword { get; set; }
  

    }
}
