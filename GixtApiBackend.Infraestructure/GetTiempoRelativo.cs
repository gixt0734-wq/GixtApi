using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GixtApiBackend.Infraestructure
{
    public static class FechaHelper
    {
        public static string GetTiempoRelativo(DateTime fecha)
    {
        var ahora = DateTime.UtcNow;
        var diferencia = ahora - fecha;

        if (diferencia.TotalSeconds < 60)
            return "Nuevo";

        if (diferencia.TotalMinutes < 60)
            return $"hace {(int)diferencia.TotalMinutes} minutos";

        if (diferencia.TotalHours < 24)
            return $"hace {(int)diferencia.TotalHours} horas";

        if (diferencia.TotalDays < 7)
            return $"hace {(int)diferencia.TotalDays} días";

        if (diferencia.TotalDays < 30)
            return $"hace {(int)(diferencia.TotalDays / 7)} semanas";

        if (diferencia.TotalDays < 365)
            return $"hace {(int)(diferencia.TotalDays / 30)} meses";

        return $"hace {(int)(diferencia.TotalDays / 365)} años";
    }
    }

}
