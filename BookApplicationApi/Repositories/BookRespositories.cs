using BookApplicationApi.Dto;
using BookApplicationApi.Interface;
using BookData;
using BookData.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace BookApplicationApi.Repositories
{
    public class BookRespositories : IBookService
    {
        private readonly BookDbContext _context;
        private readonly BooksApiUrl _url;
        private readonly IHttpClientFactory _clientFactory;

        public BookRespositories(BookDbContext context, BooksApiUrl url, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _url = url;
            _clientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Book>> AdminExportsBookData()
        {
            var result = await _context.Books.ToListAsync();
            return result;
        }

        public async Task<AdminGetBookSaveDBDto> AdminGetBookSaveDB()
        {
            Log.Information("AdminGetBookSaveDB is requested");
            var client = _clientFactory.CreateClient("RetryClient");
            var response = await client.GetAsync(_url.Link ?? string.Empty);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var books = JsonConvert.DeserializeObject<List<Book>>(json);

                // Thêm dữ liệu vào bảng Book
                if (books == null)
                {
                    var result = new AdminGetBookSaveDBDto
                    {
                        StatusCode = 400,
                        Message = "No books found in the API response."
                    };
                    return result;
                }

                // Fetch existing books from the database
                var existingBooks = await _context.Books.ToListAsync();

                // Find books that are not already in the database
                var newBooks = books.Where(b => !existingBooks.Exists(eb => eb.Id == b.Id)).ToList();

                if (newBooks.Count > 0)
                {
                    _context.Books.AddRange(newBooks);
                    await _context.SaveChangesAsync();
                    var result = new AdminGetBookSaveDBDto
                    {
                        StatusCode = 200,
                        Message = "The data has been added to the database.",
                        Book = newBooks
                    };
                    return result;
                }
                else
                {
                    var result = new AdminGetBookSaveDBDto
                    {
                        StatusCode = 200,
                        Message = "The database already contains all the books from the API."
                    };
                    return result;
                }
            }
            else
            {
                var error = new AdminGetBookSaveDBDto
                {
                    StatusCode = (int)response.StatusCode,
                    Message = "Unable to get data from API"
                };
                return error;
            }
        }


    }
}
