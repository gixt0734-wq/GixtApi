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

    public class LocationDTO
    {
        [Key]
        public Guid user_id { get; set; }
        public string street { get; set; }
        public string neighborhood { get; set; }
        public string house_number { get; set; }
        public string state { get; set; }
        public string city  { get; set; }
        public string country { get; set; }
        public string reference { get; set; }
        public IFormFile image { get; set; }
        public string maps_address { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        

    }

}
