using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GixtApiBackend.Application.DTos
{

    public class CostsDtos
    {
        [Key]
        public Guid job_id { get; set; }
        public decimal labor_cost { get; set; }
        public decimal km_cost { get; set; }
        public decimal materials { get; set; }
        public decimal iva { get; set; }
        public decimal total { get; set; }


        public List<MaterialsDtos> materiales { get; set; }

    }

}
