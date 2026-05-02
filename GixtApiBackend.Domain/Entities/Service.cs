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
    [Table("services")]
    public class Service
    {
        [Key]
        public Guid service_id { get; set; }
        public string service_name { get; set; }
        public string description { get; set; }
        public decimal labor_price { get; set; } 
        public int rating { get; set; }
        public string image_url { get; set; }
        public int duration_hours { get; set; }
        public Guid worker_id { get; set; }
        public int category_id { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }
}
