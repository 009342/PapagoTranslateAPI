using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
