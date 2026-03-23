using System.ComponentModel.DataAnnotations;

namespace ZuriFluxAPI.DTOs
{
    public class RegisterDeviceTokenDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [RegularExpression("^(android|ios)$",
            ErrorMessage = "Platform must be android or ios.")]
        public string Platform { get; set; }
    }

    public class NotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}