using BookstoreApi.Models;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookstoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersService _ordersService;
        private readonly BooksService _booksService;
        public OrdersController(OrdersService ordersService, BooksService booksService)
        {
            _ordersService = ordersService;
            _booksService = booksService;
        }

        [HttpGet]
        public async Task<List<Order>> Get()
        {
            var allOrder = await _ordersService.GetAsync();
            return allOrder;
        }
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Order>> Get(string id)
        {
            var order = await _ordersService.GetAsync(id);
            if (order is null)
            {
                return NotFound();
            }
            return order;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Order newOrder)
        {
            // Generate a new ObjectId for each OrderItem in the Order
            newOrder.Id = null;
            foreach (var orderItem in newOrder.OrderItemsList)
            {
                var book = await _booksService.GetAsync(orderItem.Book.Id);
                orderItem.Book = book;
            }
            await _ordersService.CreateAsync(newOrder);
            return Ok(newOrder);
        }
    

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Order updatedOrder)
        {
            var order = await _ordersService.GetAsync(id);
            if (order is null){
                return NotFound();
            }
            await _ordersService.UpdateAsync(id, updatedOrder);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _ordersService.GetAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            await _ordersService.RemoveAsync(id);

            return NoContent();
        }
    }
}
