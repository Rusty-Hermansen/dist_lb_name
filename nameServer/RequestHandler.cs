using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System;


class RequestHandler
{
    public List<IPEndPoint> availableDataNodes;
    public byte[] requestData;
    public IPEndPoint requester;
    private IPEndPoint? loadBalancer;

    private List<string> requestFormats = new List<string> { "^[a-z=+:[0-9]+$", "^[a-z]+$", "^[0-9]+$" };
    public RequestHandler(List<IPEndPoint> AvailableDataNodes, byte[] RequestData, IPEndPoint Requester, IPEndPoint LoadBalancer)
    {
        availableDataNodes = AvailableDataNodes;
        requestData = RequestData;
        requester = Requester;
        loadBalancer = LoadBalancer;

    }

    private byte[] createUDP(IPEndPoint ticketEndpoint)
    {
        UdpClient ticketUDP = new UdpClient();
        ticketUDP.Send(requestData, requestData.Length, ticketEndpoint);
        return ticketUDP.Receive(ref ticketEndpoint);
    }

    private void sendClientResponse(byte[] responseData, IPEndPoint client)
    {
        UdpClient clientUDP = new UdpClient();
        clientUDP.Send(responseData, responseData.Length, client);



    }
    public Func<int> buildTaskFunction(string request)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        for (int i = 0; i < requestFormats.Count; i++)
        {

            Regex check = new Regex(requestFormats[i]);
            if (check.IsMatch(request))
            {
                switch (i)
                {
                    case 0:
                        return () =>
                        {
                            byte[] responseData;

                            Console.WriteLine("Sending request to dataNode");
                            string name = request.Split(':')[0];
                            int hash = name.GetHashCode();
                            IPEndPoint dataNode = availableDataNodes[Math.Abs(hash) % availableDataNodes.Count]; //do mod math to determine which datanode to send name and number to
                          
                            responseData = createUDP(dataNode);
                            sendClientResponse(responseData, requester);

                       

                            return 0;
                        };
                    case 1:
                        return () =>
                        {
                            byte[] responseData;

                            Console.WriteLine("Sending request to dataNode");
                            string name = request.Split(':')[0];
                            int hash = name.GetHashCode();
                            IPEndPoint dataNode = availableDataNodes[Math.Abs(hash) % availableDataNodes.Count]; //do mod math to determine which datanode to send name and number to
                   
                            requestData = createUDP(dataNode);
                            responseData = createUDP(loadBalancer);

                            sendClientResponse(responseData, requester);
                            return 1;
                        };
                    case 2:
                        return () =>
                        {
                            byte[] responseData;
                            responseData = createUDP(loadBalancer);
                            sendClientResponse(responseData, requester);
                            return 2;
                        };
                }
            }


        }
        return () => { return -1; };
    }
}

















