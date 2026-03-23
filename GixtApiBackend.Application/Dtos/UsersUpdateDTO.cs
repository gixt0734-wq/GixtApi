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

    public class UserUpdateDTO
    {
        [Key]
        public Guid? user_id {  get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public IFormFile? image { get; set; }
        public string phone { get; set; }
        public string gender { get; set; }
        public DateOnly birth_date { get; set; }
    }

}
