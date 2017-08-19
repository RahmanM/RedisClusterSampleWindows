using System;
using System.Xml.Serialization;
using CacheManager.Core;
using ObjectDumper;

namespace Redis.Sample
{

    class Program
    {

        static void Main(string[] args)
        {
            // PlainRedisWorker.MainSub();

            CacheManagerWorker.MainSub();

            Console.ReadLine();
        }
    }

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

public class Dumper
{
    public static void Print(string input)
    {
        Console.WriteLine(input);
    }

    public static void Print(Customer input)
    {
        var result = ObjectDumper<Customer>.Dump(input);
        Console.WriteLine(result);
    }

    public static Customer GetCustomer()
    {
        return new Customer()
        {
            Id = 1,
            Name = "rahman",
            Address = new Address() { City = "sydney", Line1 = "address 1", Line2 = "address 2" },
        };
    }
}

[Serializable]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
}

[Serializable]
public class Address
{
    public string Line1 { get; set; }
    public string Line2 { get; set; }
    public string City { get; set; }
}



namespace ObjectDumper
{
    public sealed class ObjectDumper<T> where T : class
    {

        #region Default Constructor

        private ObjectDumper()
        {

        }

        #endregion

        #region Publics

        public static string Dump(T objectToDump)
        {
            if (objectToDump == null)
            {
                throw new ArgumentNullException("objectToDump");
            }

            var objectToString = ObjectToString(objectToDump);
            return objectToString;
        }

        #endregion

        #region Privates

        private static string ObjectToString(T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var textWriter = new System.IO.StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        #endregion

    }
}