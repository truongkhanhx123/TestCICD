using BookApplicationApi.Dto;
using BookData.Entities;
namespace BookApplicationApi.Interface
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> AdminExportsBookData();
        Task<AdminGetBookSaveDBDto> AdminGetBookSaveDB();
    }
}
