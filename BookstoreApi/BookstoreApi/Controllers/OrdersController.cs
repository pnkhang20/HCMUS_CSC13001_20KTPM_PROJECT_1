using BookstoreApi.Models;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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

            // variable to mark for updatedOrder is done
            var isDone = updatedOrder.OrderIsDone;
            var customerName = updatedOrder.CustomerName;
            var customerPhone = updatedOrder.CustomerPhone;
            var customerShippingAddress = updatedOrder.ShippingAddress;
            var newOrderItems = new List<OrderItem>();

            // item list of the order
            var oderItemList = order.OrderItemsList;
            // Remove existing order items that are not in the edited order item list
            foreach (var existingOrderItem in order.OrderItemsList.ToList())
            {
                if (!updatedOrder.OrderItemsList.Any(item => item.Book.Id == existingOrderItem.Book.Id))
                {
                    var book = await _booksService.GetAsync(existingOrderItem.Book.Id);

                    // Update the book quantity based on the quantity of the existing order item
                    book.Quantity += existingOrderItem.Quantity;

                    // Check if the order is done and update the book's total sold if it is
                    if (isDone)
                    {
                        book.TotalSold -= existingOrderItem.Quantity;
                    }
                    await _booksService.UpdateAsync(book.Id, book);

                    // Remove the existing order item from the original order item list
                    order.OrderItemsList.Remove(existingOrderItem);
                }
            }


            foreach (var updatedOrderItem in updatedOrder.OrderItemsList)
            {
                var book = await _booksService.GetAsync(updatedOrderItem.Book.Id);

                var existingOrderItem = order.OrderItemsList.FirstOrDefault(item => item.Book.Id == updatedOrderItem.Book.Id);

                if (existingOrderItem != null)
                {

                    // Update the book quantity based on the difference between the current order item quantity and the updated order item quantity
                    var quantityDifference = updatedOrderItem.Quantity - existingOrderItem.Quantity;

                    // Update the quantity of the existing order item
                    existingOrderItem.Quantity = updatedOrderItem.Quantity;


                    if (quantityDifference > book.Quantity)
                    {
                        return BadRequest($"Not enough stock for book {book.Title} ({book.Id}). Available stock: {book.Quantity}");
                    }
                    book.Quantity -= quantityDifference;
                    existingOrderItem.Book.Quantity -= quantityDifference;
                    // Check if the order is done and update the book's total sold if it is
                    if (isDone)
                    {
                        book.TotalSold += updatedOrderItem.Quantity;
                        existingOrderItem.Book.TotalSold += updatedOrderItem.Quantity;
                    }
                    await _booksService.UpdateAsync(book.Id, book);
                }

                else
                {
                    // If the updated order item is not in the order yet, add it and update the book quantity accordingly
                    if (updatedOrderItem.Quantity > book.Quantity)
                    {
                        return BadRequest($"Not enough stock for book {book.Title} ({book.Id}). Available stock: {book.Quantity}");
                    }

                    book.Quantity -= updatedOrderItem.Quantity;
                    var newOrderItem = new OrderItem
                    {
                        Book = book,
                        Quantity = updatedOrderItem.Quantity
                    };
                    order.OrderItemsList.Add(newOrderItem);
                    // Check if the order is done and update the book's total sold if it is
                    if (isDone)
                    {
                        book.TotalSold += updatedOrderItem.Quantity;
                        newOrderItem.Book.TotalSold += updatedOrderItem.Quantity;
                    }
                    await _booksService.UpdateAsync(book.Id, book);
                }


            }

            order.OrderIsDone = isDone;
            order.CustomerName = customerName;
            order.CustomerPhone = customerPhone;
            order.ShippingAddress = customerShippingAddress;

            // Recalculate the total price of the order
            order.TotalPrice = order.OrderItemsList.Sum(item => item.Book.Price * item.Quantity);

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

            if (order.OrderIsDone)
            {
                return BadRequest("Cannot delete an order that is marked as done.");
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
