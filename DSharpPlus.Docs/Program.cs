using Microsoft.DocAsCode;
namespace DSharpPlus
{
    public class Program
    {
        public static async Task Main(string[] args) => await Docset.Build("docfx.json");
    }
}
