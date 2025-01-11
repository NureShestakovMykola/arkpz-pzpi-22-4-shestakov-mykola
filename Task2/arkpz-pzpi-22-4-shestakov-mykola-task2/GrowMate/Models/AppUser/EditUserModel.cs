namespace GrowMate.Models.AppUser
{
    public class EditUserModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public IFormFile? NewImage { get; set; }
    }
}
