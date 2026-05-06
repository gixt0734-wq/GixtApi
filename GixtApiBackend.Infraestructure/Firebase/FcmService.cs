using FirebaseAdmin.Messaging;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class FcmService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FcmService(
        AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;

    }
    //Notificaciones clasicas
    //Notificaciones al usuarios
    public async Task SendNotificationByUser(Guid id, string title, string body, string type)
    {
        var user = (
              from u in _context.users
              join s in _context.sessions
              on u.user_id equals s.user_id
              where u.user_id == id && u.is_active == true
              select s.token_fcm
              ).FirstOrDefaultAsync();

        if (await user == null)
            return;


        await SendNotificationAsync(
                await user,
                title,
               body, type
            );
    }
    //Notifiaciones hacia trabajadores
    public async Task SendNotificationByWorker(Guid id, string title, string body, string type)
    {
        var user = (
              from w in _context.workers
              join u in _context.users on w.user_id equals u.user_id
              join s in _context.sessions
              on u.user_id equals s.user_id
              where w.worker_id == id && u.is_active == true
              select s.token_fcm
              ).FirstOrDefaultAsync();

        if (await user == null)
            return;


        await SendNotificationAsync(
                await user,
                title,
               body, type
            );
    }

    // Notificaciones Express
    //Propuestas de trabajadores
    public async Task sendNotificationByExpress(Guid id, String username, Guid worker_id, string title, string body, decimal km_cost, decimal labor_price)
    {
        var user = await (
              from e in _context.express
              join u in _context.users on e.client_id equals u.user_id
              join s in _context.sessions
              on u.user_id equals s.user_id
              where e.express_id == id && u.is_active == true
              select new { u.image_url, s.token_fcm }
              ).FirstOrDefaultAsync();

        var worker = await (
              from w in _context.workers 
              join u in _context.users on w.user_id equals u.user_id
              join s in _context.sessions
              on u.user_id equals s.user_id
              where w.worker_id == worker_id && u.is_active == true
              select new { u.image_url, w.rating }
              ).FirstOrDefaultAsync();

        if (user == null)
            return;

        var request = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var imgs = baseUrl + worker.image_url;


        await SendNotificationExpressAsync(user.token_fcm, imgs, worker.rating, id, worker_id, username, labor_price, km_cost, title, body);
    }
    // Notificaciones de Nuevos Express hacia trabajadores
    public async Task SendNotificationByExpress(int id_category, string title, string body, Guid serviceId, Guid client_id)
    {
        var tokens = await (
            from s in _context.services
            join w in _context.workers on s.worker_id equals w.worker_id
            join u in _context.users on w.user_id equals u.user_id
            join ses in _context.sessions on u.user_id equals ses.user_id
            where s.category_id == id_category
                  && s.is_active == true
                  && w.is_active == true
                  && u.is_active == true
                  && ses.token_fcm != null
            select ses.token_fcm
        )
        .Distinct()
        .ToListAsync();

        var user = await (
              from u in _context.users
              where u.user_id == client_id && u.is_active == true
              select new { u.image_url, u.username }
              ).FirstOrDefaultAsync();

        if (tokens.Count == 0)
            return;

        var request = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var imgs = baseUrl + user.image_url;

        await SendNotificationsAsync(tokens, imgs, user.username, title, body, serviceId);
    }


    // Clase de notificacion clasica
    public async Task SendNotificationAsync(
        string token,
        string title,
        string body,
        string type)
    {
        var message = new Message()
        {
            Token = token,

            Notification = new Notification
            {
                Title = title,
                Body = body
            },

            Data = new Dictionary<string, string>()
            {
                { "type", type },
            },

            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "Notificaciones",
                    Sound = "default",
                    ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                }
            },

            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "apns-priority", "10" } // alta prioridad iOS
                },
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = title,
                        Body = body
                    },
                    Sound = "default",
                    ContentAvailable = true,
                    MutableContent = true
                }
            }
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }


    // Notificacion de propuestas para usuario

    public async Task SendNotificationExpressAsync(
        string token,
        string img,
        int rating,
        Guid expressid,
        Guid workerid,
        string username,
        decimal labor_price,
        decimal km_cost,
        string title,
        string body)
    {
        var message = new Message()
        {
            Token = token,

            Notification = new Notification
            {
                Title = title,
                Body = body
            },

            Data = new Dictionary<string, string>()
            {
                { "type", "express" },
                {"username" , username },
                {"image", img },
                {"rating",rating.ToString() },
                { "expressid",expressid.ToString() },
                {"workerid",workerid.ToString() },
                {"labor_price",labor_price.ToString() },
                {"km_cost",km_cost.ToString() },
                { "serviceType","express"},
            },

            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "Notificaciones",
                    Sound = "default",
                    ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                }
            },

            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "apns-priority", "10" } // alta prioridad iOS
                },
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = title,
                        Body = body
                    },
                    Sound = "default",
                    ContentAvailable = true,
                    MutableContent = true
                }
            }
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    // Noptifiacaciones hacia trabajadores que tengan un servicio registrado con el id enviado
    public async Task SendNotificationsAsync(
        List<string> tokens,
        string img,
        string username,
        string title,
        string body,
        Guid serviceId)
    {
        var message = new MulticastMessage()
        {
            Tokens = tokens,

            Notification = new Notification
            {
                Title = title,
                Body = body
            },

            Data = new Dictionary<string, string>()
            {
                { "type", "service" },
                {"img",img },
                {"username",username },
                { "serviceId", serviceId.ToString()},
                { "serviceType","express"}, 
            },

            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    ChannelId = "Notificaciones",
                    Sound = "default",
                    ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                }
            },

            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "apns-priority", "10" }
                },
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = title,
                        Body = body
                    },
                    Sound = "default",
                    ContentAvailable = true,
                    MutableContent = true
                }
            }
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }
}

