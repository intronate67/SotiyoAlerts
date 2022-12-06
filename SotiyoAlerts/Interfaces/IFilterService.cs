using SotiyoAlerts.Data.Models;
using SotiyoAlerts.Models.zkilllboard;

namespace SotiyoAlerts.Interfaces
{
    public interface IFilterService
    {
        Filter GetFilter(long filterId);
        SubFilter GetSubFilter(long subFilterId);
    }
}