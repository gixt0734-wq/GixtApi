using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Application.DTos
{
    public class UserLoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? DeviceId { get; set; }      // ID único del teléfono
        public string? DeviceName { get; set; }    // Nombre del dispositivo
        public string? TokenFcm { get; set; }      // Token push notifications
    }
}
