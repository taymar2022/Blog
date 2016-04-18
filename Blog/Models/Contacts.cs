using System;
using System.ComponentModel.DataAnnotations;

namespace FirstTest_MVC.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Phone { get; set; }
        public DateTime MessageSent { get; set; }
    }
}