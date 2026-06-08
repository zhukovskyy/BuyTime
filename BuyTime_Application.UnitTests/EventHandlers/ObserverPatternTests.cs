using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.EventHandlers;
using BuyTime_Application.Events;
using BuyTime_Infrastructure.Repositories;
using BuyTime_Infrastructure.Services;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BuyTime_Application.UnitTests.EventHandlers;

public class ObserverPatternTests
{
    [Fact]
    public async Task Publisher_BookingService_ShouldSendEventToMediator()
    {
        var mockMediator = new Mock<IMediator>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockBookingRepo = new Mock<IBookingRepository>();
        var mockTimeslotRepo = new Mock<ITimeSlotRepository>();

        mockUnitOfWork.Setup(u => u.Booking).Returns(mockBookingRepo.Object);
        mockUnitOfWork.Setup(u => u.Timeslot).Returns(mockTimeslotRepo.Object);

        mockTimeslotRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new BuyTime_Domain.Entities.Timeslot { IsAvailable = true, Price = 100 });

        mockBookingRepo.Setup(r => r.AddAsync(It.IsAny<BuyTime_Domain.Entities.Booking>()))
            .ReturnsAsync(Guid.NewGuid());

        var service = new BookingService(mockUnitOfWork.Object, mockMediator.Object);

        await service.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid(), "Повідомлення", "Hash", "Wallet");

        mockMediator.Verify(m => m.Publish(It.IsAny<BookingCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Observer_EventHandler_ShouldReactToEventAndSendMessage()
    {
        var mockTelegram = new Mock<ITelegramService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockBookingRepo = new Mock<IBookingRepository>();
        var mockTimeslotRepo = new Mock<ITimeSlotRepository>();
        var mockUserRepo = new Mock<IUserRepository>();

        mockUnitOfWork.Setup(u => u.Booking).Returns(mockBookingRepo.Object);
        mockUnitOfWork.Setup(u => u.Timeslot).Returns(mockTimeslotRepo.Object);
        mockUnitOfWork.Setup(u => u.User).Returns(mockUserRepo.Object);

        var handler = new BookingCreatedEventHandler(mockTelegram.Object, mockUnitOfWork.Object);
        var eventData = new BookingCreatedEvent(Guid.NewGuid());

        var expertId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var expert = new BuyTime_Domain.Entities.User { Id = expertId, TelegramChatId = "123456789" };
        var student = new BuyTime_Domain.Entities.User { Id = studentId, FirstName = "Олександр" };
        var timeslot = new BuyTime_Domain.Entities.Timeslot { Id = Guid.NewGuid(), ExpertId = expertId };
        var booking = new BuyTime_Domain.Entities.Booking { Id = eventData.BookingId, StudentId = studentId, TimeslotId = timeslot.Id };

        mockBookingRepo.Setup(r => r.GetByIdAsync(eventData.BookingId)).ReturnsAsync(booking);
        mockTimeslotRepo.Setup(r => r.GetByIdAsync(booking.TimeslotId)).ReturnsAsync(timeslot);
        mockUserRepo.Setup(r => r.GetByIdAsync(expertId)).ReturnsAsync(expert);
        mockUserRepo.Setup(r => r.GetByIdAsync(studentId)).ReturnsAsync(student);

        await handler.Handle(eventData, CancellationToken.None);

        mockTelegram.Verify(t => t.SendMessageAsync(expert.TelegramChatId, It.IsAny<string>()), Times.Once);
    }
}