using System.Linq;
using SotiyoAlerts.Data;
using SotiyoAlerts.Data.Models;
using SotiyoAlerts.Interfaces;

namespace SotiyoAlerts.Services
{
    public class FilterService : IFilterService
    {
        private readonly SotiyoAlertsDb _ctx;

        public FilterService(SotiyoAlertsDb ctx)
        {
            _ctx = ctx;
        }

        public Filter GetFilter(long filterId)
        {
            return _ctx.Filters.FirstOrDefault(f => f.Id == filterId);
        }

        public SubFilter GetSubFilter(long subFilterId)
        {
            return _ctx.SubFilters.FirstOrDefault(f => f.Id == subFilterId);
        }
    }
}