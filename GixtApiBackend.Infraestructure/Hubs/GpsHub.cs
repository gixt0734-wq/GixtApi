using Microsoft.AspNetCore.SignalR;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using GixtApiBackend.Domain.Entities;

public class GpsHub : Hub
{
    /// Cache en memoria: última ubicación (opcional)
    private static readonly ConcurrentDictionary<Guid, (double lat, double lng)> _lastLocations
        = new();

    /// 🔹 Cliente entra al grupo de un usuario
    public async Task JoinGroup(Guid id)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
    }

    /// 🔹 Flutter envía ubicación
    public async Task SendLocation(Guid id, double lat, double lng)
    {
        // Guardar última ubicación (por si la necesitas después)
        _lastLocations[id] = (lat, lng);

        // 🔥 Enviar a TODOS los clientes conectados
        await Clients.All.SendAsync(
            "ReceiveLocation",
            id,
            lat,
            lng
        );

        // 🔥 Enviar SOLO a los que estén suscritos a ese ID (grupo)
        await Clients.Group(id.ToString()).SendAsync(
            "ReceiveLocationId",
            id,
            lat,
            lng
        );
    }

}