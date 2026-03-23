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
    [Table("favorite")]
    public class Favorite
    {
        [Key]
        public Guid favorite_id { get; set; }
        public Guid user_id { get; set; }
        public Guid service_id { get; set; }
        public bool is_active { get; set; }
    }
}
