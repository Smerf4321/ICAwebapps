using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.ViewModels
{
    public class GuestBookingVM
    {
        [Required]
        public string EventTitle { get; set; }

        public int CustomerId { get; set; }

        public bool Attended { get; set; }

    }
}
