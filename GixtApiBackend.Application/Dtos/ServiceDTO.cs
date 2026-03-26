using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GixtApiBackend.Application.DTos
{
    public class ServiceDTO
    {

        public string service_name { get; set; }
        public string description { get; set; }
        public decimal labor_price { get; set; }
        public int duration_hours { get; set; }
        public IFormFile image { get; set; }

        [FromForm(Name = "images")]
        public List<IFormFile> images { get; set; }

        public Guid user_id { get; set; }
        public int category_id { get; set; }

    }
}
