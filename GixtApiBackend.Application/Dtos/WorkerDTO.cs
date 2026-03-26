using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Application.DTos
{

    public class WorkerDTO
    {
        [Key]

        public Guid user_id { get; set; }
        public string? description { get; set; }
        public string city { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public decimal km_cost { get; set; }
        public double range_km { get;set;}
        

    }
}
