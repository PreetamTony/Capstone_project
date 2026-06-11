using HospitalManagement.BusinessLogic.Services;
using HospitalManagement.DataAccess.Context;
using HospitalManagement.DataAccess.Interfaces;
using HospitalManagement.DataAccess.Repositories;
using HospitalManagement.BusinessLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HospitalManagement.Tests;

public abstract class TestBase : IDisposable
{
    protected AppDbContext Context { get; }
    protected IUnitOfWork Uow { get; }
    protected Mock<ICurrentUserService> MockCurrentUserService { get; }
    protected Mock<INotificationService> MockNotificationService { get; }
    protected Mock<IQueueService> MockQueueService { get; }
    protected Mock<ISlotEngine> MockSlotEngine { get; }
    protected Mock<IBillingService> MockBillingService { get; }

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        MockCurrentUserService = new Mock<ICurrentUserService>();
        Context = new AppDbContext(options);
        Uow = new UnitOfWork(Context);

        MockNotificationService = new Mock<INotificationService>();
        MockBillingService = new Mock<IBillingService>();
        MockQueueService = new Mock<IQueueService>();
        MockSlotEngine = new Mock<ISlotEngine>();
    }

    protected Mock<ILogger<T>> CreateLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
