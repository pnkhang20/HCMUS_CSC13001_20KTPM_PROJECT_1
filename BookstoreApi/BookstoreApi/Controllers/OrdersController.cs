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

        // Get all orders
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

        // Create a new order
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] Order newOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderItemsList = new List<OrderItem>();

            foreach (var requestItem in newOrder.OrderItemsList)
            {
                // Find the corresponding Book Item
                var book = await _booksService.GetAsync(requestItem.Book.Id);

                if (book == null)
                {
                    return BadRequest($"Book with Id {requestItem.Book.Id} not found.");
                }

                // Check if the requested quantity is greater than the available quantity
                if (requestItem.Quantity > book.Quantity)
                {
                    return BadRequest($"Requested quantity for book {book.Title} exceeds available quantity.");
                }

                // Create a new OrderItem and assign its Book property to the retrieved book object
                var orderItem = new OrderItem
                {
                    Id = ObjectId.GenerateNewId().ToString(), // Generate new ID as a string
                    Book = book,
                    Quantity = requestItem.Quantity
                };

                book.Quantity -= requestItem.Quantity;                
                await _booksService.UpdateAsync(book.Id, book);
                orderItemsList.Add(orderItem);
            }

            var order = new Order
            {
                OrderItemsList = orderItemsList,
                ShippingAddress = newOrder.ShippingAddress,
                CustomerName = newOrder.CustomerName,
                CustomerPhone = newOrder.CustomerPhone,
                OrderIsDone = false,
                TotalPrice = orderItemsList.Sum(item => item.Book.Price * item.Quantity),
                OrderedDate = DateTime.UtcNow
            };

            await _ordersService.CreateAsync(order);            
            return Ok(order);
        }



        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Order updatedOrder)
        {
            var order = await _ordersService.GetAsync(id);
            if (order is null)
            {
                return NotFound();
            }

            var isDone = updatedOrder.OrderIsDone;
            
            // Loop through the updated order items and update the corresponding book quantities
            foreach (var updatedOrderItem in order.OrderItemsList)
            {
                var book = await _booksService.GetAsync(updatedOrderItem.Book.Id);
                if (book == null)
                {
                    return BadRequest($"Book with Id {updatedOrderItem.Book.Id} not found.");
                }

                // Check if the updated order item is already in the order or not
                var existingOrderItem = order.OrderItemsList.FirstOrDefault(item => item.Book.Id == updatedOrderItem.Book.Id);
                if (existingOrderItem != null)
                {
                    // If the updated order item is already in the order, update the book quantity based on the difference
                    var quantityDifference = updatedOrderItem.Quantity - existingOrderItem.Quantity;
                    book.Quantity -= quantityDifference;
                }
                else
                {
                    // If the updated order item is not in the order yet, add it and update the book quantity accordingly
                    var newOrderItem = new OrderItem
                    {
                        Book = book,
                        Quantity = updatedOrderItem.Quantity
                    };
                    order.OrderItemsList.Add(newOrderItem);
                    book.Quantity -= updatedOrderItem.Quantity;
                }

                // Check if the order is done and update the book's total sold if it is
                if (isDone)
                {
                    book.TotalSold += updatedOrderItem.Quantity;
                }
                await _booksService.UpdateAsync(book.Id, book);
            }
            
            order.OrderIsDone = isDone;

            // Update the order
            await _ordersService.UpdateAsync(id, order);
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

            // Update book quantities
            foreach (var orderItem in order.OrderItemsList)
            {
                var book = orderItem.Book;
                book.Quantity += orderItem.Quantity;
                await _booksService.UpdateAsync(book.Id, book);
            }
            return NoContent();
        }
    }
}
