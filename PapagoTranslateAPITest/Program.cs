using System;
using PapagoTranslateAPI;
namespace PapagoTranslateAPITest
{
    class Program
    {
        static void Main(string[] args)
        {
            PapagoTranslate papagoTranslate = new PapagoTranslate();
            Console.WriteLine(papagoTranslate.Translate("ko", "ja", "안녕하세요!","nsmt"));
            Console.ReadLine();
        }
    }
}
