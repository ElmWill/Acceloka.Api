namespace Acceloka.Api.Features.Tickets.GetAvailableTickets;

using FluentValidation;
using System.Linq.Expressions;

public class GetAvailableTicketsValidator : AbstractValidator<GetAvailableTicketsQuery>
{
    public GetAvailableTicketsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.OrderState)
            .Must(x => x == null
            || x.ToLower() == "asc"
            || x.ToLower() == "desc")
            .WithMessage("OrderState must be asc or desc");
    }
}
