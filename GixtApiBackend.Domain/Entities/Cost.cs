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
    [Table("costs")]
    public class Costs
    {
        [Key]
        public Guid cost_id { get; set; }
        public Guid job_id { get; set; }
        public decimal labor_cost { get; set; }
        public decimal km_cost { get; set; }
        public decimal materials { get; set; }
        public decimal iva { get; set; }
        public decimal total { get; set; }

    }

}
