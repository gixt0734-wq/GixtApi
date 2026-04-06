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
    [Table("payment")]
    public class Payment
    {
        [Key]
        public Guid payment_id { get; set; }
        public Guid job_id { get; set; }
        public decimal labor_cost { get; set; }
        public decimal km_cost { get; set; }
        public string payment_method { get; set; }
        public string payment_status { get; set; }
        public decimal materials { get; set; }
        public decimal iva { get; set; }
        public decimal total { get; set; }

    }

}
