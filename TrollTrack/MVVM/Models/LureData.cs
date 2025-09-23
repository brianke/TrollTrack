using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.MVVM.Models
{
    public class LureData
    {
        [Key]
        public Guid Id { get; set; }

        public String Name { get; set; }

        public List<Image> Images { get; set; }
    }
}
