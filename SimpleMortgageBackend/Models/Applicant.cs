using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleMortgageBackend.Models
{
    public class Applicant
    {
        public int Id { get; set; }
        [Display(Name = "First Name"), StringLength(50), Required]
        public string FirstName { get; set; }
        [Display(Name = "Last Name"), StringLength(50), Required]
        public string LastName { get; set; }
        [Display(Name = "Date of Birth"), DisplayFormat(DataFormatString = "dd-mm-yyyy", ApplyFormatInEditMode = true), Required]
        public DateTime DOB { get; set; }
        public string Email { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
