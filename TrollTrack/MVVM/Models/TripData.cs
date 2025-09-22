using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.MVVM.Models
{
    public class TripData
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime TripDate { get; set; } = DateTime.Now.Date;


    }
}
