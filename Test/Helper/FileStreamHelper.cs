using System.Text;

namespace Test.Helper
{
    public static class FileStreamHelper
    {
        internal static MemoryStream CreateCsvStream(string csv)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(csv));
        }
    }
}
