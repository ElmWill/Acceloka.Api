namespace Acceloka.Api.Features.Tickets.EditBookedTicket;

using FluentValidation;

public class EditBookedTicketValidator
    : AbstractValidator<EditBookedTicketCommand>
{
    public EditBookedTicketValidator()
    {
        RuleFor(Q => Q.BookedTicketId)
            .NotEmpty();

        RuleFor(Q => Q.TicketCode)
            .NotEmpty();

        RuleFor(Q => Q.Quantity)
            .GreaterThanOrEqualTo(1);
    }
}
