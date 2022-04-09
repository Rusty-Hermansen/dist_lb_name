using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


class Program
{
    static Dictionary<string, string> cache = new Dictionary<string, string>();

    public static string checkCache(string requestString)
    {
        lock (cache)
        {
            if (cache.ContainsKey(requestString))
            {
                return cache[requestString];
            }
            else
            {
                return string.Empty;
            }
        }
    }
    static void Main()
    {

        int LbPort = 9800;
        UdpClient requestsClient = new UdpClient(9877);
        UTF8Encoding encoding = new UTF8Encoding();
        var data = Encoding.UTF8.GetBytes("LBRTR");
        requestsClient.Send(data, data.Length, "255.255.255.255", LbPort);

     
        Console.WriteLine($"Started Load Balancer on port 9800");
   
        List<IPEndPoint> workers = new List<IPEndPoint>();
        int requestCount = 0;

        while (true)
        {
            IPEndPoint requester = new IPEndPoint(IPAddress.Any, 0);
            byte[] requestData = requestsClient.Receive(ref requester);
            string requestString = encoding.GetString(requestData);
            
            if (requestString == "RTR")
            {
                workers.Add(requester);
                Console.WriteLine($"Added worker to list. Current number of workers: {workers.Count}");

            }
            else
            {
                
                requestCount++;
                Task.Run(() =>
           {

               string response;
               byte[] responseData;
               string cacheCheck = checkCache(requestString);
                Console.WriteLine(cacheCheck);
               if (cacheCheck != string.Empty)
               {
                   Console.WriteLine("Retrieving response from cache.");
                   response = cacheCheck;
                   responseData = encoding.GetBytes(response);
               }
               else
               {
                   Console.WriteLine("Sending request to worker");
                   IPEndPoint worker = workers[requestCount % workers.Count]; //do mod math to determine which workers turn it is to achieve round robin
                    
                   Console.WriteLine(worker);
                   UdpClient workerUDP = new UdpClient();
                   workerUDP.Send(requestData, requestData.Length, worker);
                   responseData = workerUDP.Receive(ref worker);
                   string workerResponse = encoding.GetString(responseData);
                   Console.WriteLine("Value needs to be cached");
                   long request = long.Parse(requestString);

                   lock (cache)
                   {
                       cache.Add(requestString, workerResponse);
                   }
               }

               //sending
               UdpClient toClient = new UdpClient();
               toClient.Send(responseData, responseData.Length, requester);
           });
            }
      
        }
    }

    static List<long> GetPrimeFactors(long product)
    {
        if (product <= 1)
        {
            throw new ArgumentException();
        }
        List<long> factors = new List<long>();
        // divide out factors in increasing order until we get to 1
        while (product > 1)
        {
            for (long i = 2; i <= product; i++)
            {
                if (product % i == 0)
                {
                    product /= i;
                    factors.Add(i);
                    break;
                }
            }
        }
        return factors;
    }
}

