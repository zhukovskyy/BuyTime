using BuyTime_Application.Booking.Command.CancelBooking;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Dto;
using ErrorOr;
using FluentAssertions;
using Moq;
using Xunit;

namespace BuyTime_Application.UnitTests.Booking.Command.CancelBooking;

public class CancelBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITonContractService> _tonContractServiceMock;
    private readonly CancelBookingCommandHandler _handler;

    public CancelBookingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tonContractServiceMock = new Mock<ITonContractService>();

        _handler = new CancelBookingCommandHandler(
            _unitOfWorkMock.Object,
            _tonContractServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenBookingDoesNotExist()
    {
        var command = new CancelBookingCommand(
            BookingId: Guid.NewGuid(),
            CancellationMessage: "Передумав",
            TriggeredByUserId: Guid.NewGuid()
        );

        _unitOfWorkMock.Setup(u => u.Booking.GetByIdAsync(command.BookingId))
            .ReturnsAsync((BuyTime_Domain.Entities.Booking)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Booking.NotFound");

        _tonContractServiceMock.Verify(
            t => t.GenerateCancelBookingPayloadAsync(It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never);

        _tonContractServiceMock.Verify(
            t => t.GenerateClaimRefundPayloadAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPassIsStudentFalseToTonService_WhenExpertCancels()
    {
        var expertId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var command = new CancelBookingCommand(
            BookingId: bookingId,
            CancellationMessage: "Вибачте, не зможу провести заняття",
            TriggeredByUserId: expertId
        );

        var timeslot = new BuyTime_Domain.Entities.Timeslot
        {
            Id = Guid.NewGuid(),
            ExpertId = expertId,
            Price = 100,
            StartTime = DateTime.UtcNow.AddDays(2)
        };

        var booking = new BuyTime_Domain.Entities.Booking
        {
            Id = bookingId,
            StudentId = studentId,
            TimeslotId = timeslot.Id,
            TimeSlot = timeslot,
            Status = BuyTime_Domain.Constants.Status.Confirmed,
            ContractAddress = "EQ_TestContract"
        };

        var expectedPayload = new TonConnectPayloadDto { ContractAddress = "EQ_TestContract" };

        _unitOfWorkMock.Setup(u => u.Booking.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _tonContractServiceMock.Setup(t => t.GenerateCancelBookingPayloadAsync(false, booking.ContractAddress))
            .ReturnsAsync(expectedPayload);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedPayload);

        _tonContractServiceMock.Verify(
            t => t.GenerateCancelBookingPayloadAsync(false, booking.ContractAddress),
            Times.Once);

        booking.Cancellation.Should().NotBeNull();
        booking.Cancellation.CancelledByUserId.Should().Be(expertId);
    }

    [Fact]
    public async Task Handle_ShouldSplitFunds5050_WhenStudentCancelsConfirmedBookingBetween24And48Hours()
    {
        var studentId = Guid.NewGuid();
        var expertId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        decimal timeslotPrice = 100m;

        var command = new CancelBookingCommand(
            BookingId: bookingId,
            CancellationMessage: "Не виходить приєднатися",
            TriggeredByUserId: studentId
        );

        var timeslot = new BuyTime_Domain.Entities.Timeslot
        {
            Id = Guid.NewGuid(),
            ExpertId = expertId,
            Price = timeslotPrice,
            StartTime = DateTime.UtcNow.AddHours(30)
        };

        var booking = new BuyTime_Domain.Entities.Booking
        {
            Id = bookingId,
            StudentId = studentId,
            TimeslotId = timeslot.Id,
            TimeSlot = timeslot,
            Status = BuyTime_Domain.Constants.Status.Confirmed,
            ConfirmationMessage = "Чекаю на вас",
            ContractAddress = "EQ_TestContract"
        };

        var expectedPayload = new TonConnectPayloadDto { ContractAddress = "EQ_TestContract" };

        _unitOfWorkMock.Setup(u => u.Booking.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // скасовує студент
        _tonContractServiceMock.Setup(t => t.GenerateCancelBookingPayloadAsync(true, booking.ContractAddress))
            .ReturnsAsync(expectedPayload);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse("Handler should successfully process the cancellation");
        result.Value.Should().BeEquivalentTo(expectedPayload);

        booking.Cancellation.Should().NotBeNull();
        booking.Cancellation.RefundAmountToStudent.Should().Be(50m, "Student should get 50% back if cancelled between 24 and 48 hours");
        booking.Cancellation.CompensationAmountToExpert.Should().Be(50m, "Expert should get 50% compensation if student cancels between 24 and 48 hours");
    }
}