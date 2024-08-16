using BookApplicationApi.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using testbook.ConfigurationClasses;
using testbook.Data;
using testbook.DTO;
using testbook.ModelData;

namespace testbook.Controllers
{
    [ApiController]
    [Route("[Controller]/[action]")]
    public class Bookcontroller : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;
        private readonly Appsetting _appsetting;
        private readonly IHttpClientFactory _clientFactory;
        public Bookcontroller(ApplicationDbContext context, IHttpClientFactory clientFactory, IOptions<Appsetting> appsetting, IBookService bookService)
        {
            _context = context;
            _clientFactory = clientFactory;
            _appsetting = appsetting.Value;
            _bookService = bookService;
        }





        //GET DATA FROM LINK AND SAVE DATA IN POSTGRESSQL
        //[Authorize(Policy = "RequireAdminRole")]
        [HttpPost(Name = "/AdminGetBookSaveDB")]
        public async Task<IActionResult> AdminGetBookSaveDB()
        {
            Log.Information("AdminGetBookSaveDB is requested");
            var result = await _bookService.AdminGetBookSaveDB();

            if (!string.IsNullOrEmpty(result.Message))
            {
                if (result.Book != null && result.Book.Any())
                {
                    Log.Information("The following books have been added to the database: {@AddedBooks}", result.Book);
                    return StatusCode(result.StatusCode, new { message = result.Message, result.Book });
                }
                else
                {
                    Log.Information(result.Message);
                    return StatusCode(result.StatusCode, new { message = result.Message });
                }
            }
            else
            {
                var error = StatusCode(result.StatusCode, "Unable to get data from API");
                Log.Warning("{Errorcode} Unable to get data from API", result.StatusCode);
                return error;
            }
        }






        //EXPORT BOOK (ALL BOOK)
        [HttpGet(Name = "/AdminExportsBookData")]
        [RateLimitAttribute(10, 30)]
        public async Task<ActionResult<IEnumerable<Book>>> AdminExportsBookData()
        {
            Log.Information("Received AdminExportsBookData request");
            var result = await _bookService.AdminExportsBookData();
            if (result != null && result.Any())
            {
                Log.Information("Export All Book Success");
                return Ok(result);
            }
            else
            {
                return NotFound("No books found in data");
            }
        }





        //EXPORT BOOK (WITH PAGINATION)

        [HttpGet(Name = "/ExportsBookWithPagination")]
        [RateLimitAttribute(10, 30)]
        public async Task<ActionResult<IEnumerable<Book>>> ExportsBookWithPagination(int page = 1, int booksinpage = 10)
        {
            var book = await _context.Books
            .Skip((page - 1) * booksinpage)
            .Take(booksinpage)
            .ToListAsync();
            return Ok(book);
        }





        //EXPORT BOOK BY FILTER (INPUT = TITLE AND AUTHORS)
        [HttpGet(Name = "FilterBookByTitleAndAuthors")]
        [Authorize]
        public async Task<IActionResult> FilterBookByTitleAndAuthors(string title, string authors, [FromQuery] int page = 1, [FromQuery] int booksinpage = 10)
        {
            // Tìm sách dựa trên tiêu đề
            var query = _context.Books.AsQueryable();
            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(b => b.Title.Contains(title));
            }

            // Tìm sách dựa trên tác giả
            if (!string.IsNullOrEmpty(authors))
            {
                query = query.Where(b => b.Authors.Contains(authors));
            }

            // Tính tổng số sách
            var totalBooks = await query.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalBooks / (double)booksinpage);

            // Lấy sách cho trang hiện tại
            var pagination = await query
                .Skip((page - 1) * booksinpage)
                .Take(booksinpage)
                .ToListAsync();

            // Tạo đối tượng response
            var response = new
            {
                Page = page,
                BooksPerPage = booksinpage,
                TotalPages = totalPages,
                TotalBooks = totalBooks,
                Books = pagination
            };

            // Trả về kết quả
            return Ok(response);
        }





        //EXPORT BOOK BY FILTER (INPUT = TITLE)
        [HttpGet(Name = "/FilterWithPaginationByTitle")]
        public async Task<IActionResult> FilterBooksByTitle(string title, [FromQuery] int page = 1, [FromQuery] int booksinpage = 10)
        {
            var book = _context.Books.Where(b => b.Title.Contains(title));

            // Tính tổng số sách
            var totalBooks = await book.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalBooks / (double)booksinpage);

            // Lấy sách cho trang hiện tại
            var pagination = await book
                .Skip((page - 1) * booksinpage)
                .Take(booksinpage)
                .ToListAsync();

            // Tạo đối tượng response
            var response = new
            {
                Page = page,
                BooksPerPage = booksinpage,
                TotalPages = totalPages,
                TotalBooks = totalBooks,
                Books = pagination
            };

            // Trả về kết quả
            return Ok(response);
        }





        //EXPORT BOOK BY FILTER (INPUT = AUTHORS)
        [HttpGet(Name = "/FilterWithPaginationByAuthors")]
        public async Task<IActionResult> FilterBooksByAuthors(string authors, [FromQuery] int page = 1, [FromQuery] int booksinpage = 10)
        {
            var book = _context.Books.Where(b => b.Authors.Contains(authors));

            // Tính tổng số sách
            var totalBooks = await book.CountAsync();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalBooks / (double)booksinpage);

            // Lấy sách cho trang hiện tại
            var pagination = await book
                .Skip((page - 1) * booksinpage)
                .Take(booksinpage)
                .ToListAsync();

            // Tạo đối tượng response
            var response = new
            {
                Page = page,
                BooksPerPage = booksinpage,
                TotalPages = totalPages,
                TotalBooks = totalBooks,
                Books = pagination
            };

            // Trả về kết quả
            return Ok(response);
        }





        //GET BOOK BY ID
        [HttpGet(Name = "/AdminGetIdBook")]
        public async Task<ActionResult<Book>> AdminGetIdBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }





        //CREATE BOOK
        //[Authorize]
        [HttpPut(Name = "/AdminCreateBook")]
        public async Task<IActionResult> AdminCreateBook(DtoBookInput book)
        {
            if (book == null)
            {
                return BadRequest("Book data is null");
            }

            // Đảm bảo rằng Id của book không tồn tại để tránh ghi đè dữ liệu hiện có
            var existingBook = await _context.Books.FindAsync(book.Id);
            if (existingBook != null)
            {
                return Conflict("A book with the same ID already exists.");
            }

            var newBook = new Book
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PageCount = book.PageCount,
                Authors = book.Authors,
            };

            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();

            return Ok(newBook);
        }





        // EDIT BOOK
        //[Authorize]
        [HttpPut(Name = "/UpdateBook")]
        public async Task<IActionResult> UpdateBook([FromBody] DtoBookInput updatedBook)
        {
            if (updatedBook == null)
            {
                return BadRequest("Updated book data is null");
            }

            var BookUpdate = await _context.Books.FindAsync(updatedBook.Id);

            //Điều kiện kiểm tra


            if (BookUpdate == null)
            {
                return Ok("Book does not exist");
            }

            //Lưu lại book chưa chỉnh sửa
            var bookbefore = new Book
            {
                Id = BookUpdate.Id,
                Title = BookUpdate.Title,
                Isbn = BookUpdate.Isbn,
                PageCount = BookUpdate.PageCount,
                Authors = BookUpdate.Authors,
                CreateAt = BookUpdate.CreateAt,
                UpdateAt = BookUpdate.UpdateAt
            };

            //Edit và update book
            BookUpdate.Title = updatedBook.Title;
            BookUpdate.Isbn = updatedBook.Isbn;
            BookUpdate.PageCount = updatedBook.PageCount;
            BookUpdate.Authors = updatedBook.Authors;
            BookUpdate.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            //Export ra book chưa edit và book sau khi edit để đối chiếu xem mình sửa cái gì
            var response = new
            {
                BookBefore = bookbefore,
                BookAfter = BookUpdate
            };

            return Ok(response);
        }





        // DELETE BOOK BY ID
        //[Authorize]   
        [HttpDelete(Name = "/AdminDeleteIdBook")]
        public async Task<IActionResult> AdminDeleteIdBook(int id)
        {
            // Tìm cuốn sách để xóa
            var book = await _context.Books.FindAsync(id);

            // Nếu không tìm thấy, trả về mã lỗi 404 Not Found
            if (book == null)
            {
                return NotFound();
            }

            // Lưu thông tin của cuốn sách đã bị xóa
            var deletedBook = new Book
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PageCount = book.PageCount,
                Authors = book.Authors
            };

            // Xóa cuốn sách và lưu thay đổi vào cơ sở dữ liệu
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            // Trả về kết quả 204 No Content nếu thành công
            return Ok(deletedBook);
        }





        // DELETE BOOK BY AUTHORS
        [Authorize]
        [HttpDelete(Name = "/DeleteBooksByAuthor")]
        public async Task<IActionResult> DeleteBooksByAuthor([FromBody] string author)
        {
            // Tìm các cuốn sách có tác giả cần xóa
            var booksToDelete = await _context.Books.Where(b => b.Authors.Contains(author)).ToListAsync();

            // Nếu không tìm thấy cuốn sách nào, trả về mã lỗi 404 Not Found
            if (booksToDelete.IsNullOrEmpty())
            {
                return NotFound();
            }

            // Lưu danh sách các cuốn sách đã xóa
            List<Book> deletedBooks = new List<Book>(booksToDelete);

            // Xóa các cuốn sách và lưu thay đổi vào cơ sở dữ liệu
            _context.Books.RemoveRange(booksToDelete);
            await _context.SaveChangesAsync();

            // Trả về kết quả 204 No Content nếu thành công
            return Ok(deletedBooks);
        }






        // DELETE ALL BOOK
        //[Authorize]
        [HttpDelete(Name = "/AdminDeleteALLBook")]
        public async Task<IActionResult> AdminDeleteALLBook()
        {
            var AllBook = await _context.Books.ToListAsync();
            _context.Books.RemoveRange(AllBook);
            await _context.SaveChangesAsync();
            return Ok("deletion completed");
        }


    }
}
