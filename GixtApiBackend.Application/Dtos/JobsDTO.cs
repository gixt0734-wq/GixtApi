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

    public class JobDTO
    {
        [Key]
        public Guid client_id { get; set; }
        public Guid service_id { get; set; }
        public Guid location_id { get; set; }
        public DateOnly job_date { get; set; }
        public TimeOnly job_time { get; set; }
        public string problem { get; set; }
        public int duration_minutes { get; set; }
        public string description { get; set; }
        public IFormFile image_1 { get; set; }
        public IFormFile image_2 { get; set; }
        public string payment_method { get; set; }
    }

}
