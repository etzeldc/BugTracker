using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        [StringLength(50, ErrorMessage = "The Name must be between {2} and {1} characters long.", MinimumLength = 1)]
        public string FirstName { get; set; }
        [StringLength(50, ErrorMessage = "The Name must be between {2} and {1} characters long.", MinimumLength = 5)]
        public string LastName { get; set; }
        [StringLength(50, ErrorMessage = "The Name must be between {2} and {1} characters long.", MinimumLength = 1)]
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }

        [StringLength(50, ErrorMessage = "The Email must be between {2} and {1} characters long.", MinimumLength = 5)]
        public string Email { get; set; }
    }
}