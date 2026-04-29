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

    public class ReviewDTO
    {
        [Key]
        public Guid client_id { get; set; }
        public Guid id { get; set; }
        public int rating { get; set; }
        public string? comment { get; set; }
        public int rating_worker { get; set; }
        public string? comment_worker { get; set; }
        public IFormFile? image_url { get; set; }
    }

}
