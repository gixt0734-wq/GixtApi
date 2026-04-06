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
    [Table("materials")]
    public class Materials
    {
        [Key]
        public Guid materials_id { get; set; }
        public Guid payment_id { get; set; }
        public string name { get; set; }
        public decimal cost { get; set; }

    }

}
