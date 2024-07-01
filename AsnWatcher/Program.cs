using AsnWatcher;
using AsnWatcher.Configuration;
using ASN.Infrastructure.Data.Configuration;
using ASN.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDatabase(builder.Configuration);

var host = builder.Build();

await host.SetupHockeyPoolDBAsync();

using var fileWatcher =  StartWatchingForIncomingFiles(host.Services);

host.Run();

//As in the task there was requirement for only one path to monitor I have used the same path for all the suppliers.
//If we would have to use different paths for different suppliers this should be created as some factory class which will create the FileWatcher for each supplier.
//That would also allow as well process different supplier files in different ways.
FileWatcher StartWatchingForIncomingFiles(IServiceProvider serviceProvider)
{
    var scope = serviceProvider.CreateScope(); 
    var suppliersConfig = scope.ServiceProvider.GetService<IConfiguration>().GetSection("SuppliersConfig").Get<SupplierConfig>();
    var unitOfWork = scope.ServiceProvider.GetService<UnitOfWork>();
    if (suppliersConfig == null)
    {
        throw new ArgumentNullException("SuppliersConfig is not found in the configuration file");
    }

    var fileWatcher = new FileWatcher(suppliersConfig, unitOfWork);
    fileWatcher.Watch();

    return fileWatcher;
}