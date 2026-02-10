namespace Acceloka.Api.Features.Tickets.GetBookedTicket;

using FluentValidation;

public class GetBookedTicketValidator
    : AbstractValidator<GetBookedTicketQuery>
{
    public GetBookedTicketValidator()
    {
        RuleFor(Q => Q.BookedTicketId)
            .NotEmpty()
            .WithMessage("BookedTicketId is required");
    }
}
