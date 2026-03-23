using System;
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
    public class UserDTO
    {
        [Key]
        public string password { get; set; }
        [EmailAddress]
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public IFormFile imagen { get; set; }
        public string phone { get; set; }
        public string gender { get; set; }
        public bool terms {  get; set; }
        public DateOnly birth_date { get; set; }
    }

}
