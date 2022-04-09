using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


class Program
{

    static void Main()
    {


        int LbPort = 9877;
        UdpClient Worker = new UdpClient();
        UTF8Encoding encoding = new UTF8Encoding();
        var data = Encoding.UTF8.GetBytes("RTR");
        Worker.Send(data, data.Length, "255.255.255.255", LbPort);

        Console.WriteLine($"Starting a new worker listening for requests on port: {LbPort}");

        while (true)
        {
            IPEndPoint LbTicket = new IPEndPoint(0, 0);
            byte[] requestData = Worker.Receive(ref LbTicket);

            // start a thread to respond
            Task.Run(() =>
            {
                string requestString = encoding.GetString(requestData);
                string response;
                byte[] responseData;

                long request = long.Parse(requestString);
                List<long> answer = GetPrimeFactors(request);
                response = String.Join(',', answer.Select(p => p.ToString()));

                responseData = encoding.GetBytes(response);
                    //sending
                UdpClient toLoadBalancer = new UdpClient();
                toLoadBalancer.Send(responseData, responseData.Length, LbTicket);
            });
            // var data = Encoding.UTF8.GetBytes("RTR");
            // Worker.Send(data,data.Length, "255.255.255.255",LbPort);
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

