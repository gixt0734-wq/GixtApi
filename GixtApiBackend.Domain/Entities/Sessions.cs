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
    [Table("sessions")]
    public class Session
    {
        [Key]
        public Guid session_id { get; set; }
        public Guid user_id { get; set; }
        public string device_id { get; set; }
        public string device_name { get; set; }
        public int token_version { get; set; }
        public string token_fcm { get; set; }
        public bool is_active { get; set; }
        public DateTime updated_at { get; set; }
    }
}
