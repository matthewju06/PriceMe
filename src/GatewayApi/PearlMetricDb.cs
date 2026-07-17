using Microsoft.EntityFrameworkCore;

namespace PearlMetric.GatewayApi.Data;
public class PearlMetricDb : DbContext
{
    public PearlMetricDb(DbContextOptions<PearlMetricDb> options) : base(options) { }
}