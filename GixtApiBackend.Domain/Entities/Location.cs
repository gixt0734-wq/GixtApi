using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GixtApiBackend.Application.Entities
{
    [Table("locations")]
    public class Location
    {
     [Key]
        public Guid location_id { get; set; }
        public Guid user_id { get; set; }
        public string street { get; set; }
        public string? neighborhood { get; set; }
        public string house_number { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string reference { get; set; }
        public string image_url { get; set; }
        public string maps_address { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public bool is_active { get; set; }

    }

}
