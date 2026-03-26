using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Domain.Entities
{

    [Table("advertisements")]
    public class Advertisement
    {
        [Key]
        public int advertisement_id { get; set; }
        public string name { get; set; }
        public string image_url { get; set; }
        public bool is_active { get; set; }

    }
}
