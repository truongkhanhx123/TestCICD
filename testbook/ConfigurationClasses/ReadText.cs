namespace testbook.ConfigurationClasses
{
    public static class ReadText
    {
        public static string ReadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            return File.ReadAllText(filePath).Trim(); // Đọc nội dung tệp và loại bỏ khoảng trắng đầu/cuối
        }
    }
}
