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
    [Table("users")]
    public class User
    {
        [Key]
        public Guid user_id { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string image_url { get; set; }
        public string phone { get; set; }
        public string gender { get; set; }
        public DateOnly birth_date { get; set; }
        public int rol_id { get; set; }
        public bool terms {  get; set; }
        public bool is_active { get; set; }
    }
}
