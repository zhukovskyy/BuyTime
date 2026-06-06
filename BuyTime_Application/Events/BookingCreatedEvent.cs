using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;

namespace BuyTime_Application.Events;

public record BookingCreatedEvent(Guid BookingId) : INotification;
