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
    [Table("service_images")]
    public class Service_image
    {
        [Key]
        public Guid service_image_id { get; set; }
        public string image_url { get; set; }
        public Guid service_id { get; set; }
        public bool is_active { get; set; }
    }
}
