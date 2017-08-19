using System;
using CacheManager.Core;

namespace Redis.Sample
{
    public class CacheManagerWorker
    {
        public static void MainSub()
        {
            var manager = CacheFactory.Build<Customer>(settings =>
            {
                settings
                    .WithRedisConfiguration("redis", config =>
                    {
                        config.WithAllowAdmin()
                            .WithDatabase(0)
                            .WithEndpoint("192.168.1.108", 6379);
                    })
                    .WithMaxRetries(100)
                    .WithRetryTimeout(50)
                    .WithRedisBackplane("redis")
                    .WithRedisCacheHandle("redis", true);
            });

            // add customer
            manager.Add("customer1", Dumper.GetCustomer());
            Dumper.Print(manager.Get<Customer>("customer1"));

            // add with expiration
            manager.Expire("customer1", ExpirationMode.Absolute, TimeSpan.FromSeconds(5));
            manager.OnRemoveByHandle += Manager_OnRemoveByHandle;
            //manager.OnRemove += CacheItem_Removed;
            Dumper.Print(manager.Get<Customer>("customer1"));

           var manager2 = CacheFactory.Build<CacheItem<Customer>>(settings =>
           {
               settings
                   .WithRedisConfiguration("redis", config =>
                   {
                       config.WithAllowAdmin()
                           .WithDatabase(0)
                           .WithEndpoint("localhost", 6379);
                   })
                   .WithMaxRetries(100)
                   .WithRetryTimeout(50)
                   .WithRedisBackplane("redis")
                   .WithRedisCacheHandle("redis", true);
           });
            var ci = new CacheItem<Customer>("customer2", Dumper.GetCustomer(), ExpirationMode.Absolute, new TimeSpan(0, 0, 2));
            manager2.OnRemove += CacheItem_Removed;
            manager2.Add("customer2", ci);

        }

        // NOTE: These somehow doesn't work!!!!!!

        private static void Manager_OnRemoveByHandle(object sender, CacheManager.Core.Internal.CacheItemRemovedEventArgs e)
        {
            Dumper.Print(e.Key);
        }

        private static void CacheItem_Removed(object sender, CacheManager.Core.Internal.CacheActionEventArgs e)
        {
            Dumper.Print(e.Key);
        }
    }
}
