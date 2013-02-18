using System;
using Yageo;
namespace csharpClient
{
    class MainClass
    {
        static readonly string address = "Москва, Красная площадь";
        public static void Main (string[] args)
        {
            GeoRequest y = new GeoRequest(address);
            var x = y.GeoCode();
        }
    }
}
