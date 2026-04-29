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
    [Table("reviews")]
    public class Reviews
    {
        [Key]
        public Guid review_id { get; set; }
        public Guid client_id { get; set; }
        public Guid job_id { get; set; }
        public int rating { get; set; }
        public string? comment { get; set; }
        public string? image_url { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }

}
