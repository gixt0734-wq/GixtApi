using FirebaseAdmin.Messaging;
using GixtApiBackend.Domain.Entities;
using GixtApiBackend.Infraestructure;
using Microsoft.EntityFrameworkCore;

public class FcmService
{
    private readonly AppDbContext _context;
  

    public FcmService(
        AppDbContext context)
    {
        _context = context;

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
               body,type
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
               body,type
            );
    }

    // Notificaciones Express
    //Propuestas de trabajadores
    public async Task sendNotificationByExpress(Guid id, String username, Guid worker_id, string title, string body, decimal km_cost, decimal labor_price)
    {
        var user = (
              from e in _context.express
              join u in _context.users on e.client_id equals u.user_id
              join s in _context.sessions
              on u.user_id equals s.user_id
              where e.express_id == id && u.is_active == true
              select s.token_fcm
              ).FirstOrDefaultAsync();

        if (await user == null)
            return;


        await SendNotificationExpressAsync(await user, id, worker_id, username,labor_price, km_cost, title, body);
    }
    // Notificaciones de Nuevos Express hacia trabajadores
    public async Task SendNotificationByExpress(int id_category, string title, string body, Guid serviceId)
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

        if (tokens.Count == 0)
            return;

        await SendNotificationsAsync(tokens, title, body, serviceId);
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

