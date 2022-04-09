
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

class Program
{

    static void Main()
    {
        Dictionary<string, string> storedNameFavorites = new Dictionary<string, string>();

        int LbPort = 9800;
        UdpClient DataNode = new UdpClient();
        UTF8Encoding encoding = new UTF8Encoding();
        var data = Encoding.UTF8.GetBytes("RTR");
        DataNode.Send(data, data.Length, "255.255.255.255", LbPort);

        Console.WriteLine($"Starting a new DataNode listening for requests on port: {LbPort}");

        while (true)
        {
            IPEndPoint NSTicket = new IPEndPoint(0, 0);
            byte[] ticketData = DataNode.Receive(ref NSTicket);
            string requestString = encoding.GetString(ticketData);
            Regex requestedformat1 = new Regex("^[a-z=+:[0-9]+$");

            // start a thread to respond
            if (requestedformat1.IsMatch(requestString))
            {
                Task.Run(() =>
                {
                    Console.WriteLine("Writing to dictionary " + requestString);
                    
                    string name = requestString.Split(':')[0];
                    string number = requestString.Split(':')[1];
                    lock (storedNameFavorites)
                    {
                        storedNameFavorites.Add(name, number);
                    };


                });

            }
            else
            {
                Task.Run(() =>
                {
                   Console.WriteLine("Retreiving value from dictionary " + requestString);

                    string value;
                    lock (storedNameFavorites)
                    {
                        storedNameFavorites.TryGetValue(requestString, out value);
                    }
                    byte[] responseData = encoding.GetBytes(value);
                    //sending
                    UdpClient toNameServer = new UdpClient();
                    toNameServer.Send(responseData, responseData.Length, NSTicket);
                });

            }
        }
    }


}

