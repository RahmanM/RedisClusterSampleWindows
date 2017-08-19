using System;

namespace Redis.Sample
{

    class Program
    {

        static void Main(string[] args)
        {

            // PlainRedisWorker.MainSub();

            // Using cache manager
            CacheManagerWorker.MainSub();

            Console.ReadLine();
        }
    }
}

