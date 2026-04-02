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

    public class MaterialsDtos
    {
        [Key]
  
        public string name { get; set; }
        public decimal cost {  get; set; }
    }

}
