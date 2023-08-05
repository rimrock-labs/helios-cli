namespace data_format
{
    using System.Threading.Tasks;
    using Parquet.Serialization;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Frame data = new()
            {
                ModuleName = "Foo",
                MethodName = "Bar",
                Value1 = 1,
                Value2 = 2,
            };

            await ParquetSerializer.SerializeAsync(new[] { data }, "data.parquet");
        }
    }
}