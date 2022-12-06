using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Models.zkilllboard;

namespace SotiyoAlerts.Interfaces
{
    public interface IClassificationService
    {
        Filters GetFilterFromKillmail(Killmail killmail);
        SubFilter GetSubFilterFromKillmail(Filters filter, Killmail killmail);
    }
}
