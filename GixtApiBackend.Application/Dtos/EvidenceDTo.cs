using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GixtApiBackend.Application.DTos
{
    
    public class EvidenceDTO
    {
        [Key]
        [FromForm(Name = "images")]
        public List<IFormFile> images { get; set; }
        public Guid job_id { get; set; }
        public bool is_express { get; set; }
    }
}
