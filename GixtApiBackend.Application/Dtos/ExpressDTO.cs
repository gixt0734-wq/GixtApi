using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GixtApiBackend.Application.DTos
{

    public class ExpressDTO
    {
        [Key]
        public Guid client_id { get; set; }
        public int category_id { get; set; }
        public DateOnly job_date { get; set; }
        public TimeOnly job_time { get; set; }
        public string problem { get; set; }
        public decimal labor_cost { get; set; }
        public string description { get; set; }
        public IFormFile image { get; set; }
        public string payment_method { get; set; }
        public string maps_address { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }

    }

}
