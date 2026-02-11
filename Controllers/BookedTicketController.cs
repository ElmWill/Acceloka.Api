using Acceloka.Api.Features.Tickets.BookTicket;
using Acceloka.Api.Features.Tickets.EditBookedTicket;
using Acceloka.Api.Features.Tickets.GetAvailableTickets;
using Acceloka.Api.Features.Tickets.GetBookedTicket;
using Acceloka.Api.Features.Tickets.RevokeBookedTicket;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acceloka.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class BookedTicketController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BookedTicketController(IMediator mediator)
        {
            _mediator = mediator;
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
