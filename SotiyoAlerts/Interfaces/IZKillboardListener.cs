using System.Threading;
using System.Threading.Tasks;

namespace SotiyoAlerts.Interfaces
{
    public interface IZKillboardListener
    {
        Task ConnectAsync(CancellationToken ct = default);
        Task StartListeningAsync(CancellationToken ct = default);
    }
}