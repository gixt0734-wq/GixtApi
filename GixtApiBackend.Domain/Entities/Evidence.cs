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
    [Table("evidence")]
    public class Evidence
    {
        [Key]
        public Guid evidence_id { get; set; }
        public string image_url { get; set; }
        public Guid job_id { get; set; }
    }
}
