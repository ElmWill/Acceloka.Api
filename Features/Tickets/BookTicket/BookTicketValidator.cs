namespace Acceloka.Api.Features.Tickets.BookTicket;

using FluentValidation;

public class BookTicketValidator : AbstractValidator<BookTicketCommand>
{
    public BookTicketValidator()
    {
        RuleFor(Q => Q.Tickets)
            .NotEmpty().WithMessage("Tickets cannot be empty");

        RuleForEach(Q => Q.Tickets).ChildRules(ticket =>
        {
            ticket.RuleFor(t => t.TicketCode)
                .NotEmpty().WithMessage("Ticket Code is Required");

            ticket.RuleFor(t => t.Quantity)
                .GreaterThan(0).WithMessage("Quantity mush be greater than 0");
        });
    }
}
