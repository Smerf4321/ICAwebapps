using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Staff
    {
        public int Id { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "First Aider")]
        public bool FirstAider { get; set; }

        public List<EventStaffing> Staffing { get; set; }
    }
}
