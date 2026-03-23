//using GixtApiBackend.Infrastructure;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Hosting;


//namespace GixtApiBackend.BackgroundServices
//{
//    public class NotificationWorker : BackgroundService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;

//        public NotificationWorker(IServiceScopeFactory scopeFactory)
//        {
//            _scopeFactory = scopeFactory;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await ProcessNotifications();

//                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // cada hora
//            }
//        }

//        private async Task ProcessNotifications()
//        {
//            using var scope = _scopeFactory.CreateScope();

//            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//            var fcmService = scope.ServiceProvider.GetRequiredService<FcmService>();

//            var now = DateTime.Now;

//            var today = DateOnly.FromDateTime(now);

//            var jobs = await context.jobs
//                .Where(j => j.job_status == "accepted")
//                .ToListAsync();
            

//            foreach (var job in jobs)
//            {
//                var jobDateTime = job.job_date.ToDateTime(job.job_time);
//                var services = await context.services
//                .FirstOrDefaultAsync(s => s.service_id == job.service_id);

//                // 🔥 Recordatorio cada hora si es hoy
//                if (job.job_date == today)
//                {
//                    await fcmService.SendNotificationByUser(
//                        job.client_id,
//                        "Servicio pendiente",
//                        $"Tienes un servicio pendiente: {services.service_name}", "Reminder"
//                    );

//                    await fcmService.SendNotificationByWorker(
//                        job.worker_id,
//                        "Servicio pendiente",
//                        $"Tienes un servicio programado hoy: {services.service_name}", "Reminder"
//                    );
//                }

//                // 🔥 1 hora antes
//                if (now >= jobDateTime.AddHours(-1) && now <= jobDateTime.AddMinutes(-55))
//                {
//                    await fcmService.SendNotificationByUser(
//                        job.client_id,
//                        "Servicio próximo",
//                        $"Tu servicio inicia en 1 hora: {services.service_name}", "Reminder"
//                    );

//                    await fcmService.SendNotificationByWorker(
//                        job.worker_id,
//                        "Servicio próximo",
//                        $"Tu servicio inicia en 1 hora: {services.service_name}", "Reminder"
//                    );
//                }
//            }
//        }
//    }

//}
