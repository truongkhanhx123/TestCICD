using BookApplicationApi.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using testbook.ConfigurationClasses;
using testbook.Controllers;
using testbook.Data;
using testbook.ModelData;


namespace xUnitTest.Tests
{
    public class TestBookController
    {
        private readonly Bookcontroller _controller;
        private readonly ApplicationDbContext _context;
        private readonly Appsetting _appsetting;
        private readonly Mock<IBookService> _mockBookService;
        private readonly Mock<IHttpClientFactory> _mockClientFactory;
        private static readonly string[] entities = new[] { "Test Khanh" };

        public TestBookController()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _appsetting = new Appsetting();
            _mockBookService = new Mock<IBookService>();
            _controller = new Bookcontroller(_context, _mockClientFactory.Object, Options.Create(_appsetting), _mockBookService.Object);
        }

        [Fact]
        public async Task AdminExportsBookData_ReturnsBooks()
        {
            // Arrange (Thiết lập dữ liệu ban đầu)
            _context.Books.AddRange(new Book { Id = 1, Title = "Test Book", Isbn = "2312", PageCount = 100, Authors = entities });
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.AdminExportsBookData();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var okResult = Assert.IsType<List<Book>>(actionResult.Value);
            var singleBook = Assert.Single(okResult);
            Assert.Equal("Test Book", singleBook.Title);
            // Cleanup (Dọn dẹp dữ liệu sau kiểm thử)
            _context.Books.RemoveRange(_context.Books);
            await _context.SaveChangesAsync();
        }
        [Fact]
        public async Task AdminExportsBookData_ReturnsNotFound_WhenNoBooks()
        {
            _context.Books.RemoveRange(_context.Books);
            await _context.SaveChangesAsync();
            var result = await _controller.AdminExportsBookData();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Book>>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("No books found in data", notFoundResult.Value);
        }
    }
}
