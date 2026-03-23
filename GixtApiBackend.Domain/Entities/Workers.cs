using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Domain.Entities
{
    [Table("workers")]
    public class Worker
    {
        [Key]
        public Guid worker_id { get; set; }
        public Guid user_id { get; set; }
        public string? description { get; set; }
        public int rating { get; set; }
        public string city { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public decimal labor_cost { get; set; }
        public double range_km { get; set; }
        public bool is_active { get; set; } 

    }
}
