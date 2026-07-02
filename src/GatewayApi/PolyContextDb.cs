using Microsoft.EntityFrameworkCore;

namespace GatewayApi;

public class PolyContextDb : DbContext
{
    public PolyContextDb(DbContextOptions<PolyContextDb> options) : base(options) { }
}