namespace Acceloka.Api.Features.Tickets.GetAvailableTickets;

using FluentValidation;
using System.Linq.Expressions;

public class GetAvailableTicketsValidator : AbstractValidator<GetAvailableTicketsQuery>
{
    public GetAvailableTicketsValidator()
    {
        RuleFor(Q => Q.Page)
            .GreaterThan(0);

        RuleFor(Q => Q.OrderState)
            .Must(Q => Q == null
            || Q.ToLower() == "asc"
            || Q.ToLower() == "desc")
            .WithMessage("OrderState must be asc or desc");
    }
}
