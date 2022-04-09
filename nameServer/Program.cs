using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;



class Program
{
    static Dictionary<string, string> cache = new Dictionary<string, string>();

    static void Main()
    {

        int NSPort = 9800;
        UdpClient requestsClient = new UdpClient(NSPort);
        UTF8Encoding encoding = new UTF8Encoding();

        Console.WriteLine($"Started Load Balancer on port ");

        List<IPEndPoint> datanodes = new List<IPEndPoint>();
        IPEndPoint LoadBalancer = new IPEndPoint(0, 0);
        int requestCount = 0;

        while (true)
        {
            IPEndPoint requester = new IPEndPoint(IPAddress.Any, 0);
            byte[] requestData = requestsClient.Receive(ref requester);
            string requestString = encoding.GetString(requestData);

            if (requestString == "RTR")
            {
                datanodes.Add(requester);
                Console.WriteLine($"Added worker to list. Current number of workers: {datanodes.Count}");

            }
            else if (requestString == "LBRTR")
            {
                LoadBalancer = requester;
                Console.WriteLine($"Set load balancer to {requester}");
            }
            else
            {
                Console.WriteLine("Processing request from client");
                RequestHandler handler = new RequestHandler(datanodes, requestData, requester, LoadBalancer);
                Func<int> taskFunction = handler.buildTaskFunction(requestString);
                Task.Run(taskFunction);
                requestCount++;
       
            }

        }
    }

   
}

 //         Task.Run(() =>
        //    {
        //        //hashcode of name here % by datanodes.count

        //        string response;
        //        byte[] responseData;



        //        Console.WriteLine("Sending request to worker");
        //        IPEndPoint worker = datanodes[requestCount % datanodes.Count]; //do mod math to determine which workers turn it is to achieve round robin
        //        Console.WriteLine(worker);
        //        UdpClient dataNodeUDP = new UdpClient();
        //        dataNodeUDP.Send(requestData, requestData.Length, worker);
        //        responseData = dataNodeUDP.Receive(ref worker);
        //        string workerResponse = encoding.GetString(responseData);
        //        //sending
        //        UdpClient toNameServer = new UdpClient();
        //        toNameServer.Send(responseData, responseData.Length, requester);
        //    });