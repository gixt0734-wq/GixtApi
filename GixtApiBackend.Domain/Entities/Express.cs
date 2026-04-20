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
    [Table("express")]
    public class Express
    {
        [Key]
        public Guid express_id { get; set; }
        public Guid client_id { get; set; }
        public int category_id { get; set; }
        public Guid? worker_id { get; set; }
        public DateOnly job_date { get; set; }
        public TimeOnly job_time { get; set; }
        public string problem { get; set; }
        public string description { get; set; }
        public string? description_worker { get; set; }
        public string job_status { get; set; }
        public DateTime start_job { get; set; }
        public DateTime finish_job { get; set; }
        public string image_url { get; set; }
        public string maps_address { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public bool is_active { get; set; }

    }

}
