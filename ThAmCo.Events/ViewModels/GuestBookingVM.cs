using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.ViewModels
{
    public class GuestBookingVM
    {
        public int CustomerId { get; set; }

        public int EventId { get; set; }

        public bool Attended { get; set; }

        [Required]
        public string EventTitle { get; set; }
    }
}
