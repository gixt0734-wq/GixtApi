using GixtApiBackend.Application.DTos;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.IO;
using System.Threading.Tasks;


namespace GixtApiBackend.Infraestructure.Repositories
{
    public class EvidenceRepository : IEvidenceRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageService _imageService;
        private readonly FcmService _fcmService;

        public EvidenceRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor, FcmService fcmService, ImageService imageService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _fcmService = fcmService;
            _imageService = imageService;
        }

        public async Task CreateEvidenceAsync(EvidenceDTO dto)
        {
            try
            {
                // Buscamos si ya existe evidencia registrada para este trabajo
                var existin = await _context.evidence
                    .Where(e => e.job_id == dto.job_id)
                    .FirstOrDefaultAsync();

                // Si ya existe, significa que el trabajo ya fue terminado
                if (existin != null)
                {
                    throw new InvalidOperationException("Job already completed");
                }

                // Si no llegan imágenes, no hay nada que procesar
                if (dto.images == null || dto.images.Count == 0)
                    return;

                // Lista donde acumulamos las evidencias a insertar
                var evidencesToAdd = new List<Evidence>();

                // Recorremos cada imagen recibida
                foreach (var e in dto.images)
                {
                    // Saltamos las imágenes nulas
                    if (e == null)
                        continue;

                    // Guardamos la imagen física y obtenemos su URL
                    var img = await _imageService.SaveImageAsync(e, "img_evidences");

                    // Creamos la entidad de evidencia asociada al trabajo
                    evidencesToAdd.Add(new Evidence
                    {
                        image_url = img,
                        job_id = dto.job_id
                    });
                }

                // Si se generaron evidencias, las insertamos en la base de datos
                if (evidencesToAdd.Count > 0)
                {
                    await _context.evidence.AddRangeAsync(evidencesToAdd);
                    await _context.SaveChangesAsync();
                }

                // Caso: el trabajo es de tipo express
                if (dto.is_express)
                {
                    // Buscamos el registro express por su id
                    var existing = await _context.express.FindAsync(dto.job_id);
                    if (existing == null)
                        throw new InvalidOperationException("Job not found");

                    // Marcamos el trabajo como finalizado
                    existing.job_status = "finalized";

                    // Notificamos al cliente que su servicio express terminó
                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio finalizado 🎉",
                        $"El servicio '{existing.problem}' ha sido completado.", "Express"
                    );
                }
                // Caso: el trabajo es de tipo normal
                else
                {
                    // Buscamos el trabajo por su id
                    var existing = await _context.jobs.FindAsync(dto.job_id);
                    if (existing == null)
                        throw new InvalidOperationException("Job not found");

                    // Marcamos el trabajo como finalizado
                    existing.job_status = "finalized";

                    // Notificamos al cliente que su servicio terminó
                    await _fcmService.SendNotificationByUser(
                        existing.client_id,
                        "Servicio finalizado 🎉",
                        $"El servicio '{existing.problem}' ha sido completado.", "Job"
                    );
                }

                // Guardamos el cambio de estado del trabajo
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Error al persistir los cambios en la base de datos
                throw new Exception("Error saving evidence to the database", ex);
            }
            catch (Exception)
            {
                // Relanzamos cualquier otro error para que lo maneje la capa superior
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetAllCostAsync()
        {
            try
            {
                // Obtenemos todos los registros de pago de la base de datos
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
                throw new Exception("Error retrieving the payment list", ex);
            }
        }


        public async Task<object?> GetCostByIdAsync(Guid id)
        {
            try
            {
                // Construimos la URL base a partir del contexto de la petición actual
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                // Consultamos el pago y traemos sus materiales asociados
                var result = await (
                    from c in _context.payment
                    select new
                    {
                        c.materials,
                        c.job_id,
                        c.payment_id,
                        c.labor_cost,
                        c.km_cost,
                        c.iva,
                        // Obtenemos la lista de materiales que pertenecen a este pago
                        List_Materials = _context.materials
                            .Where(m => m.payment_id == c.payment_id)
                            .ToList(),
                    }
                ).FirstOrDefaultAsync();

                // Si no se encontró ningún registro, devolvemos null
                if (result == null)
                    return null;

                // Devolvemos el resultado encontrado
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
