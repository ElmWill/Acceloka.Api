namespace Acceloka.Api.Features.Tickets.RevokeBookedTicket;

using FluentValidation;

public class RevokeBookedTicketValidator
    : AbstractValidator<RevokeBookedTicketCommand>
{
    public RevokeBookedTicketValidator()
    {
        RuleFor(Q => Q.BookedTicketId)
            .NotEmpty();

        RuleFor(Q => Q.TicketCode)
            .NotEmpty();

        RuleFor(Q => Q.Quantity)
            .GreaterThan(0);
    }
}
