using Acceloka.Api.Features.Tickets.BookTicket;
using Acceloka.Api.Features.Tickets.EditBookedTicket;
using Acceloka.Api.Features.Tickets.GetAvailableTickets;
using Acceloka.Api.Features.Tickets.GetBookedTicket;
using Acceloka.Api.Features.Tickets.RevokeBookedTicket;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acceloka.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Produces("application/json", "application/problem+json")]
    public class TicketController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TicketController(IMediator mediator)
        {
            _mediator = mediator;
        }
        // GET: api/<TicketController>
        [HttpGet("get-available-ticket")]
        public async Task<IActionResult> GetAvailableTicket(
            [FromQuery] GetAvailableTicketsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("book-ticket")]
        public async Task<IActionResult> BookTicket(
            [FromBody] BookTicketCommand command)
        {
            var result = await _mediator.Send(command);
            return Created("", result);
        }

        [HttpGet("get-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> GetBookedResult(
            Guid bookedTicketId)
        {
            var result = await _mediator.Send(
                new GetBookedTicketQuery
                {
                    BookedTicketId = bookedTicketId
                });
            return Ok(result);
        }

        [HttpDelete("revoke-ticket/{bookedTicketId}/{ticketCode}/{quantity}")]
        public async Task<IActionResult> RevokeBookedTicket(
            Guid bookedTicketId,
            string ticketCode,
            int quantity)
        {
            var result = await _mediator.Send(
                new RevokeBookedTicketCommand
                {
                    BookedTicketId = bookedTicketId,
                    TicketCode = ticketCode,
                    Quantity = quantity
                });

            return Ok(result);
        }

        [HttpPut("edit-booked-ticket")]
        public async Task<IActionResult> EditBookedTicket(
            [FromBody] EditBookedTicketCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
