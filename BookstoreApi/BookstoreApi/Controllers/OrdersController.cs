using BookstoreApi.Models;
using BookstoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

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

            // Check if OrderItemsList is null
            if (newOrder.OrderItemsList == null)
            {
                // Create a new order with empty OrderItemsList
                var to_add = new Order
                {
                    ShippingAddress = newOrder.ShippingAddress,
                    CustomerName = newOrder.CustomerName,
                    CustomerPhone = newOrder.CustomerPhone,
                    OrderIsDone = false,
                    TotalPrice = 0,
                    OrderedDate = DateTime.UtcNow
                };

                await _ordersService.CreateAsync(to_add);
                return Ok(to_add);
            }

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


        [HttpGet("revenue/day")]
        public async Task<ActionResult<List<RevenueByDay>>> GetRevenueByDay([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var orders = await _ordersService.GetOrdersBetweenDatesAsync(fromDate, toDate);

            var revenueByDay = new List<RevenueByDay>();
            var currentDate = fromDate.Date;

            while (currentDate <= toDate.Date)
            {
                var ordersOnCurrentDate = orders.Where(order => order.OrderedDate.Date == currentDate);
                var totalRevenue = ordersOnCurrentDate.Sum(order => order.OrderItemsList.Sum(orderItem => orderItem.Book.Price * orderItem.Quantity));

                revenueByDay.Add(new RevenueByDay
                {
                    Date = currentDate,
                    TotalRevenue = totalRevenue
                });

                currentDate = currentDate.AddDays(1);
            }

            return revenueByDay;
        }

        [HttpGet("revenue/month")]
        public async Task<ActionResult<List<RevenueByMonth>>> GetRevenueByMonth([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var orders = await _ordersService.GetOrdersBetweenDatesAsync(fromDate, toDate);

            var revenueByMonth = orders.GroupBy(
                order => new { order.OrderedDate.Year, order.OrderedDate.Month },
                (key, group) => new RevenueByMonth
                {
                    Year = key.Year,
                    Month = key.Month,
                    TotalRevenue = group.Sum(order => order.OrderItemsList.Sum(orderItem => orderItem.Book.Price * orderItem.Quantity))
                }
            ).ToList();

            return revenueByMonth;
        }

        [HttpGet("revenue/year")]
        public async Task<ActionResult<List<RevenueByYear>>> GetRevenueByYear([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var orders = await _ordersService.GetOrdersBetweenDatesAsync(fromDate, toDate);

            var revenueByYear = orders.GroupBy(
                order => order.OrderedDate.Year,
                (key, group) => new RevenueByYear
                {
                    Year = key,
                    TotalRevenue = group.Sum(order => order.OrderItemsList.Sum(orderItem => orderItem.Book.Price * orderItem.Quantity))
                }
            ).ToList();

            return revenueByYear;
        }

        //[HttpGet("books/sold")]
        //public async Task<ActionResult<IEnumerable<Book>>> GetSoldBooks()
        //{
        //    var orders = await _ordersService.GetAsync(); // get all orders from the database
        //    var soldBooks = new List<Book>(); // initialize an empty list to store the sold books

        //    foreach (var order in orders)
        //    {
        //        foreach (var orderItem in order.OrderItemsList)
        //        {
        //            // check if the book has already been sold before
        //            var soldBook = soldBooks.FirstOrDefault(b => b.Id == orderItem.Book.Id);
        //            if (soldBook != null)
        //            {
        //                // increment the total sold count for the existing book
        //                soldBook.TotalSold += orderItem.Quantity;
        //            }
        //            else
        //            {
        //                // add the book to the list of sold books with the current quantity
        //                soldBooks.Add(new Book
        //                {
        //                    Id = orderItem.Book.Id,
        //                    Title = orderItem.Book.Title,
        //                    Author = orderItem.Book.Author,
        //                    Price = orderItem.Book.Price,
        //                    Quantity = orderItem.Quantity,
        //                    Cover = orderItem.Book.Cover,
        //                    Category = orderItem.Book.Category,
        //                    TotalSold = orderItem.Quantity
        //                });
        //            }
        //        }
        //    }

        //    return soldBooks;
        //}

        [HttpGet("sold")]
        public async Task<ActionResult<IEnumerable<BookSold>>> GetBooksSoldByDateRange(DateTime fromDate, DateTime toDate)
        {
            var orders = await _ordersService.GetOrdersBetweenDatesAsync(fromDate, toDate);
            var bookSoldList = new List<BookSold>();

            foreach (var order in orders)
            {
                foreach (var orderItem in order.OrderItemsList)
                {
                    var book = orderItem.Book;
                    var existingBookSold = bookSoldList.FirstOrDefault(x => x.BookName == book.Title);

                    if (existingBookSold != null)
                    {
                        existingBookSold.Sold += orderItem.Quantity;
                    }
                    else
                    {
                        var newBookSold = new BookSold
                        {
                            BookName = book.Title,
                            Sold = orderItem.Quantity
                        };

                        bookSoldList.Add(newBookSold);
                    }
                }
            }

            return bookSoldList;
        }


    }
}
