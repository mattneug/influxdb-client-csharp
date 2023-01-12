using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static NodaTime.TimeZones.ZoneEqualityComparer;

namespace InfluxDB.Client
{
    public interface IDashboardApi
    {
        public enum Sortby { ID, CreatedAt, UpdatedAt }

        public Task<Dashboards> ListDashboardsAsync(
            bool descending=false,
            string[] dashboardIds=null,
            int limit=20,
            int offset=0,
            string org=null,
            string orgId=null,
            string owner=null,
            Sortby sortby=Sortby.CreatedAt);
    }

    public class DashboardApi : IDashboardApi
    {
        private readonly InfluxDBClientOptions _options;
        private readonly DashboardsService _service;

        internal String SortbyToString(IDashboardApi.Sortby sortby)
        {
            switch(sortby)
            {
                case IDashboardApi.Sortby.CreatedAt: return "CreatedAt";
                case IDashboardApi.Sortby.UpdatedAt: return "UpdatedAt";
                case IDashboardApi.Sortby.ID: return "ID";
                default: throw new ArgumentException("unexpected <sortby> value");
            }
        }

        protected internal DashboardApi(InfluxDBClientOptions options, DashboardsService service)
        {
            Arguments.CheckNotNull(options, nameof(options));
            Arguments.CheckNotNull(service, nameof(service));

            _options = options;
            _service = service;
        }

        public async Task<Dashboards> ListDashboardsAsync(bool descending = false, string[] dashboardIds = null, int limit = 20, int offset = 0, string org = null, string orgId = null, string owner = null, IDashboardApi.Sortby sortby = IDashboardApi.Sortby.CreatedAt)
        {
            return await _service.GetDashboardsAsync(zapTraceSpan: null, offset: offset, limit: limit, descending: descending, owner: owner, sortBy: SortbyToString(sortby), orgID: orgId, org: org);
        }
    }
}
