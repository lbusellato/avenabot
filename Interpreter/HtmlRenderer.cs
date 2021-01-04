using CoreHtmlToImage;
using System.IO;

namespace avenabot.Interpreter
{
    public static class HtmlRenderer
    {
        public static void Convert(string source, int n)
        {
            var converter = new HtmlConverter();
            var bytes = converter.FromHtmlString(source);
            string file = n switch
            {
                0 => "gironeA.jpg",
                1 => "gironeB.jpg",
                2 => "gironeC.jpg",
                3 => "gironeF.jpg",
                _ => "classifica.jpg",
            };
            File.WriteAllBytes(file, bytes);
        }
    }
}
