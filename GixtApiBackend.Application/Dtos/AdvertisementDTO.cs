using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GixtApiBackend.Domain.Entities
{
   
    public class AdvertisementDTO
    {
        [Key]
        public IFormFile image { get; set; }
        public string name { get; set; }
       
    }
}
