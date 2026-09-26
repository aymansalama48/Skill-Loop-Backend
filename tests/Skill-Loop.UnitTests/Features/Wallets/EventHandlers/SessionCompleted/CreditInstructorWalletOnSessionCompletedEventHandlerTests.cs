namespace Skill_Loop.UnitTests.Features.Wallets.EventHandlers.SessionCompleted;

using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Features.Wallets.EventHandlers.SessionCompleted;
using Skill_Loop.Domain.Entities.Sessions;
using Skill_Loop.Domain.Entities.Sessions.Events;
using Skill_Loop.Domain.Entities.Wallets;
using Skill_Loop.UnitTests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CreditInstructorWalletOnSessionCompletedEventHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CreditInstructorWalletOnSessionCompletedEventHandler _handler;

    public CreditInstructorWalletOnSessionCompletedEventHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new CreditInstructorWalletOnSessionCompletedEventHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenSessionDoesNotExist_ReturnsEarly()
    {
        var notification = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 20));

        await _handler.Handle(notification, CancellationToken.None);

        var wallets = await _dbContext.ToListAsync(_dbContext.UserWallets, CancellationToken.None);
        wallets.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPriceIsZero_DoesNotCreateWallet()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Free session");
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(session.Id, session.InstructorId, Guid.NewGuid(), 0));

        await _handler.Handle(notification, CancellationToken.None);

        var wallets = await _dbContext.ToListAsync(_dbContext.UserWallets, CancellationToken.None);
        wallets.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidEvent_CreatesWalletAndAddsCredits()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Live session", creditsPrice: 35);
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(session.Id, session.InstructorId, Guid.NewGuid(), 35));

        await _handler.Handle(notification, CancellationToken.None);

        var wallet = await _dbContext.FirstOrDefaultAsync(
            _dbContext.UserWallets.Where(w => w.UserId == session.InstructorId), CancellationToken.None);

        wallet.Should().NotBeNull();
        wallet!.Balance.Should().Be(35);
    }

    [Fact]
    public async Task Handle_WhenWalletAlreadyExists_AddsCreditsToExistingBalance()
    {
        var session = Session.Create(Guid.NewGuid(), Guid.NewGuid(), "Live session", creditsPrice: 20);
        _dbContext.Add(session);
        _dbContext.Add(UserWallet.Create(session.InstructorId, 80));
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var notification = new DomainEventNotification<SessionCompletedDomainEvent>(
            new SessionCompletedDomainEvent(session.Id, session.InstructorId, Guid.NewGuid(), 20));

        await _handler.Handle(notification, CancellationToken.None);

        var wallet = await _dbContext.FirstOrDefaultAsync(
            _dbContext.UserWallets.Where(w => w.UserId == session.InstructorId), CancellationToken.None);

        wallet!.Balance.Should().Be(100);
    }
}
