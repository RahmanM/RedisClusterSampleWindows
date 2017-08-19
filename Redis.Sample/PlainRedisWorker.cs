using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Threading;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Redis.Sample
{

    public class PlainRedisWorker
    {
        static ConnectionMultiplexer redisConnections = GetConnection(); // = ConnectionMultiplexer.Connect("192.168.1.108:6378");

        private static ConnectionMultiplexer  GetConnection()
        {
            if (redisConnections == null)
            {
                ConfigurationOptions option = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    EndPoints = { "192.168.1.108" }
                };

                redisConnections = ConnectionMultiplexer.Connect(option);
            }

            return redisConnections;
        }

        public static void MainSub()
        {

            CacheInvalidation();

            // Add item to cache
            Set("c1", new Customer()
            {
                Id = 1,
                Name = "rahman",
                Address = new Address() { City = "sydney", Line1 = "address 1", Line2 = "address 2" },
            });

            // Get item from cache
            var customer = Get<Customer>("c1");
            Console.WriteLine(ObjectDumper.ObjectDumper<Customer>.Dump(customer));

            // Add item to cache with expiry
            Set("c2", new Customer()
            {
                Id = 1,
                Name = "rahman 2",
                Address = new Address() { City = "sydney", Line1 = "address 1", Line2 = "address 2" },
            }, new TimeSpan(0, 0, 2));

            var customer2 = Get<Customer>("c2");
            Console.WriteLine(ObjectDumper.ObjectDumper<Customer>.Dump(customer2));

            // Add another item to be removed
            Set("c3", new Customer()
            {
                Id = 1,
                Name = "rahman 3",
                Address = new Address() { City = "sydney", Line1 = "address 1", Line2 = "address 2" },
            }, new TimeSpan(0, 0, 2));

            Console.WriteLine("Sleeping");
            Thread.Sleep(2500);


            Console.WriteLine("Finished!");
            Console.ReadLine();

        }

        public static void Set<T>(string key, T objectToCache) where T : class
        {
            var db = redisConnections.GetDatabase();
            db.StringSet(key, JsonConvert.SerializeObject(objectToCache
                        , Formatting.Indented
                        , new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        }));
        }

        public static void Set<T>(string key, T objectToCache, TimeSpan expiry) where T : class
        {
            var db = redisConnections.GetDatabase();
            db.StringSet(key, JsonConvert.SerializeObject(objectToCache
                        , Formatting.Indented
                        , new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        }), expiry);
        }


        public static T Get<T>(string key) where T : class
        {
            var db = redisConnections.GetDatabase();

            var redisObject = db.StringGet(key);
            if (redisObject.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(redisObject
                        , new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                            PreserveReferencesHandling = PreserveReferencesHandling.Objects
                        });
            }
            else
            {
                return (T)null;
            }
        }

        private static void CacheInvalidation()
        {

            IDatabase db = redisConnections.GetDatabase();
            ISubscriber subscriber = redisConnections.GetSubscriber();

            subscriber.Subscribe("__keyspace@0__:*", (channel, value) =>
            {
                Console.WriteLine("Channel=> " + channel);

                if (channel.ToString().IndexOf("__keyspace@0__:") > -1)
                {
                    Console.WriteLine("Item removed from cache:" + channel.ToString().Split(':')[1]);

                    //byte[] v = GetInstanceField(typeof(RedisValue), value, "valueBlob");
                    //// this is not working@@
                    //var itemRemoved = FromByteArray<Customer>(v);
                    //Console.WriteLine(ObjectDumper.ObjectDumper<Customer>.Dump(itemRemoved));
                }
            }
            );
        }

        internal static byte[] GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return (byte[])field.GetValue(instance);
        }

        public static T FromByteArray<T>(byte[] data)
        {
            try
            {
                if (data == null)
                    return default(T);
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    object obj = bf.Deserialize(ms);
                    return (T)obj;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default(T);
        }
    }
}
