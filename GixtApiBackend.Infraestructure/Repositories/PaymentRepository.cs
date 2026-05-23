using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Application.DTos;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Infraestructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public PaymentRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task UpdatePaymentAsync(PaymentDtos dto)
        {
            try
            {
                // Buscamos el pago asociado al trabajo
                var existing = _context.payment
                    .Where(p => p.job_id == dto.job_id)
                    .FirstOrDefault();

                // Caso: el trabajo es de tipo express
                if (dto.isexpress)
                {
                    // Buscamos el trabajo express
                    var express = _context.express
                        .Where(p => p.express_id == dto.job_id)
                        .FirstOrDefault();

                    // Si ya tiene diagnóstico, no permitimos diagnosticar de nuevo
                    if (express.description_worker != null)
                    {
                        throw new Exception("Job already diagnosed");
                    }

                    // Guardamos el diagnóstico del trabajador y cambiamos el estado
                    express.description_worker = dto.description;
                    express.job_status = "diagnosing";

                    // Notificamos al cliente que ya hay diagnóstico
                    await _fcmService.SendNotificationByUser(
                        express.client_id,
                        "El trabajador ya diagnosito ",
                        $"El trabajador de '{express.problem}' ya diagnositico tu problema.", "Express"
                    );
                }
                // Caso: el trabajo es de tipo normal
                else
                {
                    // Buscamos el trabajo
                    var job = _context.jobs
                        .Where(p => p.job_id == dto.job_id)
                        .FirstOrDefault();

                    // Si ya tiene diagnóstico, no permitimos diagnosticar de nuevo
                    if (job.description_worker != null)
                    {
                        throw new Exception("Job already diagnosed");
                    }

                    // Guardamos el diagnóstico del trabajador y cambiamos el estado
                    job.description_worker = dto.description;
                    job.job_status = "diagnosing";

                    // Notificamos al cliente que ya hay diagnóstico
                    await _fcmService.SendNotificationByUser(
                        job.client_id,
                        "El trabajador ya diagnosito ",
                        $"El trabajador de '{job.problem}' ya diagnositico tu problema.", "Job"
                    );
                }

                // Si existe el pago, actualizamos sus costos y materiales
                if (existing != null)
                {
                    existing.materials = dto.materials;
                    existing.total = dto.total;
                    existing.iva = dto.iva;
                    existing.labor_cost = dto.labor_cost;
                }

                // Guardamos los cambios del diagnóstico y del pago
                await _context.SaveChangesAsync();

                // Si no llegan materiales, terminamos aquí
                if (dto.materiales == null || dto.materiales.Count == 0)
                    return;

                // Lista donde acumulamos los materiales a insertar
                var materialsToAdd = new List<Materials>();

                // Recorremos cada material recibido
                foreach (var mat in dto.materiales)
                {
                    // Saltamos materiales nulos o sin nombre
                    if (mat == null || string.IsNullOrWhiteSpace(mat.name))
                        continue;

                    // Creamos el material asociado al pago
                    materialsToAdd.Add(new Materials
                    {
                        name = mat.name,
                        cost = mat.cost,
                        payment_id = existing.payment_id
                    });
                }

                // Si se generaron materiales, los insertamos en la base de datos
                if (materialsToAdd.Count > 0)
                {
                    await _context.materials.AddRangeAsync(materialsToAdd);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                // Error al persistir el diagnóstico, el pago o los materiales en la base de datos
                throw new Exception("Error updating the payment in the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentAsync()
        {
            try
            {
                // Obtenemos todos los pagos de la base de datos
                var cost = await _context.payment.ToListAsync();

                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Devolvemos la lista de pagos
                return cost;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar los pagos
                throw new Exception("Error retrieving the payments list", ex);
            }
        }

        public async Task<object?> GetPaymentByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el pago que corresponde al trabajo y traemos sus materiales
                var result = await (
                    from c in _context.payment
                    where c.job_id == id
                    select new
                    {
                        c.materials,
                        c.job_id,
                        c.payment_id,
                        c.labor_cost,
                        c.km_cost,
                        c.iva,
                        // Lista de materiales asociados a este pago
                        List_Materials = _context.materials
                            .Where(m => m.payment_id == c.payment_id)
                            .ToList(),
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró el pago, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado
                return result;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier error al consultar el pago por id
                throw new Exception("Error retrieving the payment by id", ex);
            }
        }
    }
}