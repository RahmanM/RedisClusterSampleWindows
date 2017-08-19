# RedisClusterSampleWindows
Sample configuring and using Redis cluster in c# and in Windows

Stemps:

- Download Redis
- Copy Redis to appropriate machines
- Change the config files 
> Ip Address
> Port number
> cluster-enabled yes
> cluster-config-file nodes-6380.conf
> cluster-node-timeout 15000
> appendonly yes
- Run all the nodes via redis-server.exe <config-name>.conf
- Let the clusters know about each other. 

Let is say we have 3 master clusters 192.168.1.107:6379, 192.168.1.107:6380 and 192.168.1.108:6379
redis-cli.exe -c -h 192.168.1.107 -p 6379 cluster meet 192.168.1.107 6380
redis-cli.exe -c -h 192.168.1.107 -p 6379 cluster meet 192.168.1.108 6379

- Finally set the slots
FOR /l %i in (0,1,5000) DO redis-cli.exe -h 192.168.1.107 -p 6379 CLUSTER ADDSLOTS %i
FOR /l %i in (5000,1,10923) DO redis-cli.exe -h 192.168.1.107 -p 6380 CLUSTER ADDSLOTS %i
FOR /l %i in (10924,1,16383) DO redis-cli.exe -h 192.168.1.108 -p 6379 CLUSTER ADDSLOTS %i	

- To test:
> Run the redis-cli via redis-cli -h <node-ip-address> -p <bide-port-number> -c
> Type CLUSTER INFO

- Run the .net client to add sample data ....
