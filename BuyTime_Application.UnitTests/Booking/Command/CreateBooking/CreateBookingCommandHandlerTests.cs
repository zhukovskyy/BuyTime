using BuyTime_Application.Booking.Command.CreateBooking;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Application.Common.Settings;
using BuyTime_Application.Dto;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.NetworkInformation;
using Xunit;

namespace BuyTime_Application.UnitTests.Booking.Command.CreateBooking;

public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBookingService> _bookingServiceMock;
    private readonly Mock<ITonContractService> _tonContractServiceMock;
    private readonly IOptions<PlatformSettings> _platformSettings;

    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingServiceMock = new Mock<IBookingService>();
        _tonContractServiceMock = new Mock<ITonContractService>();

        _platformSettings = Options.Create(new PlatformSettings
        {
            CommissionPercent = 5
        });

        _handler = new CreateBookingCommandHandler(
            _unitOfWorkMock.Object,
            _bookingServiceMock.Object,
            _tonContractServiceMock.Object,
            _platformSettings);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenStudentHasNoWalletForThisCurrency()
    {
        var command = new CreateBookingCommand(
            StudentId: Guid.NewGuid(),
            TimeslotId: Guid.NewGuid(),
            MessageToExpert: "Хочу урок!"
        );

        var timeslot = new BuyTime_Domain.Entities.Timeslot
        {
            Id = command.TimeslotId,
            Currency = "TON",
            Price = 90
        };

        var studentWallets = new List<BuyTime_Domain.Entities.Wallet>
        {
            new BuyTime_Domain.Entities.Wallet { Network = "ETH", Address = "0xABC123" },
            new BuyTime_Domain.Entities.Wallet { Network = "SOL", Address = "123"}
        };

        _unitOfWorkMock.Setup(u => u.Timeslot.GetByIdAsync(command.TimeslotId))
            .ReturnsAsync(timeslot);

        _unitOfWorkMock.Setup(u => u.Wallet.GetAllByUserIdAsync(command.StudentId))
            .ReturnsAsync((ErrorOr<List<BuyTime_Domain.Entities.Wallet>>)studentWallets);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("StudentWallet.Missing");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndPayload_WhenEverythingIsCorrect()
    {
        var command = new CreateBookingCommand(
            StudentId: Guid.NewGuid(),
            TimeslotId: Guid.NewGuid(),
            MessageToExpert: "Хочу урок!"
        );

        var expertWalletAddress = "EQ_ExpertWallet";
        var studentWalletAddress = "EQ_StudentWallet";

        var timeslot = new BuyTime_Domain.Entities.Timeslot
        {
            Id = command.TimeslotId,
            Currency = "TON",
            Price = 90,
            ExpertWalletAddress = expertWalletAddress,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(1)
        };

        var studentWallets = new List<BuyTime_Domain.Entities.Wallet>
        {
            new BuyTime_Domain.Entities.Wallet { Network = "TON", Address = studentWalletAddress }
        };

        var expectedPayload = new TonConnectPayloadDto
        {
            ContractAddress = "EQ_NewContractAddress"
        };

        var expectedBookingId = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.Timeslot.GetByIdAsync(command.TimeslotId))
            .ReturnsAsync(timeslot);

        _unitOfWorkMock.Setup(u => u.Wallet.GetAllByUserIdAsync(command.StudentId))
            .ReturnsAsync((ErrorOr<List<BuyTime_Domain.Entities.Wallet>>)studentWallets);

        _tonContractServiceMock.Setup(t => t.GenerateCreateBookingPayloadAsync(
                studentWalletAddress,
                expertWalletAddress,
                timeslot.StartTime,
                timeslot.EndTime,
                It.IsAny<decimal>()))
            .ReturnsAsync(expectedPayload);

        _bookingServiceMock.Setup(b => b.CreateBookingAsync(
                command.StudentId,
                command.TimeslotId,
                command.MessageToExpert,
                expectedPayload.ContractAddress,
                studentWalletAddress))
            .ReturnsAsync(expectedBookingId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.BookingId.Should().Be(expectedBookingId);
        result.Value.TonPayload.Should().BeEquivalentTo(expectedPayload);

        _tonContractServiceMock.Verify(t => t.GenerateCreateBookingPayloadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>()),
            Times.Once);
    }
}