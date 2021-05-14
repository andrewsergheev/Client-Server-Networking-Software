//Demonstrate Sockets
using System;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace location
{
    /// <summary>
    /// This class is doing all the client requests
    /// </summary>
    public class Whois
    {
        public static string response; //this is variable used to output any message to the text box in windows form as result
        public static string debugResponse;//this is variable used to output debugging message to the text box in windows form
        /// <summary>
        /// The Main method handles clients input which are args and processing them
        /// </summary>
        /// <param name="args"></param>
        static public void Main(string[] args)
        {
            //Below are the variables which been set default and can be chancged at UI
            string userName = null;
            string locationInput = null;
            string serverName = "whois.net.dcs.hull.ac.uk";
            int portNumber = 43;
            string protocol = "whois";
            bool debug = false;
            int timeout = 1000;
            

            //When argument is 0 the UI shall start
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LocationForm());//running windos form calls LocationForm
            }
            //When arguents are different than 0 code below will take place and will process without UI
            else
            {
                try
                {
                    //This loop loops arguments to pass data
                    for (int i = 0; i < args.Length; i++)
                    {
                        //The switch below is ceared to assign arguments a role
                        switch (args[i])
                        {
                            case "-h"://When "-h" is passed thats mean next arguments is a server name
                                serverName = args[i + 1];
                                i++;
                                break;
                            case "-p"://When "-p" is passed thats mean next arguments is a oirt number
                                portNumber = int.Parse(args[i + 1]);
                                i++;
                                break;
                            case "-t"://When "-t" is passed thats mean next arguments is timeout time
                                timeout = int.Parse(args[i + 1]);
                                i++;
                                break;
                            case "-d"://When "-d" is passed thats mean debuging should start
                                debug = true;
                                break;
                            case "wre@n"://When "wre@n" is passed this means whois protocol should be used
                                protocol = "whois";
                                break;
                            case "-h1"://When "-h1" is passed this means HTTP/1.1 protocol should be used
                                protocol = args[i];
                                break;
                            case "-h0"://When "-ho" is passed this means HTTP/1.0 protocol should be used
                                protocol = args[i];
                                break;
                            case "-h9"://When "-h9" is passed this means HTTP/0.9 protocol should be used
                                protocol = args[i];
                                break;
                            default:
                                if (userName == null)
                                {
                                    userName = args[i];//assigning the user name as first input as deafault
                                }
                                else if (locationInput == null)
                                {
                                    locationInput = args[i];//assigning the location as second input as deafault
                                }
                                else
                                {
                                    Console.WriteLine("ERROR: Too many arguments");
                                    return;
                                }

                                break;
                        }

                    }
                }
                //Error handling for the switch process
                catch (Exception e)
                {
                    Console.WriteLine("Argument error: " + e);
                    return;
                }
            }
            //When debug is true the string with every variables should be outputed
            if (debug)
            {
                Console.WriteLine($"userName={userName},locationInput={locationInput},serverName={serverName},portNumber={portNumber},protocol={protocol}, Timeout={timeout}");
                debugResponse = $"userName={userName},locationInput={locationInput},serverName={serverName},portNumber={portNumber},protocol={protocol}, Timeout={timeout}";
            }
            //When user name is not unouted the error message sgould be outputed
            if (userName == null)
            {
                Console.WriteLine("No user name");
                response = "No user name";
                return;
            }
            try
            {
                //Creating connection
                TcpClient client = new TcpClient();
                client.ReceiveTimeout = timeout;//setting up recieve timeout
                client.SendTimeout = timeout;//setting uo send timeout
                client.Connect(serverName, portNumber);
                StreamWriter sw = new StreamWriter(client.GetStream());//request which will send staff to server
                StreamReader sr = new StreamReader(client.GetStream());//request which will read staff from server
                //The switch below is for protacols which can be chosen
                switch (protocol)
                {
                    //The "-h1" protocol is used for HTTP/1.1 protocol
                    case "-h1":
                        // if the client wants to do look up
                        if (locationInput == null && portNumber == 80)// if location input is null and port number is 80
                        {
                            sw.WriteLine($"GET /?name={userName} HTTP/1.1\r\nHost: {serverName}\r\n");//Request to server for current look up protocol
                            sw.Flush();//This method is sending staff to server

                            
                            string line = sr.ReadLine().Trim();//When server answer is recieved this variable read the line of the response
                            if (line == "HTTP/1.1 404 Not Found")//When server do not find user's location the mesage not found should be outputed
                            {

                                Console.WriteLine(line + "\r\nContent-Type: text/plain\r\n\r\n");//not found message
                                response = line + "\r\nContent-Type: text/plain\r\n\r\n";//this variable keeps message to UI text box to be outputed

                            }
                            else
                            {
                                while ((line != "") == true)//checks blank line
                                {
                                    line = sr.ReadLine().Trim();//Thus reads next line
                                }
                                
                                Console.Write(userName + " is ");//This outputs username
                                response = userName + " is " ;//Same output for UI
                                try
                                {
                                    int c;
                                    while ((c = sr.Read()) > 0)
                                    {
                                        Console.Write((char)c);
                                    }

                                }
                                catch { }
                                finally
                                {
                                    sw.Close();
                                    sr.Close();
                                    client.Close();
                                }
                            }
                        }
                        else if (locationInput == null && portNumber != 80)// if location input is null and port number is not 80
                        {
                            sw.WriteLine($"GET /?name={userName} HTTP/1.1\r\nHost: {serverName}\r\n");//Request to server for current look up protocol
                            sw.Flush();//This method is sending staff to server
                            string serverResponse = sr.ReadToEnd();//When server send respond this variable is reading answer till end
                            string[] sections = serverResponse.Split(new string[] { "\r\n" }, StringSplitOptions.None);//This line is splitting the respond by lines
                            locationInput = sections[3];//last line is the acctual location variable 

                            if (serverResponse == "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n")//When server do not find user's location the mesage not found should be outputed
                            {

                                Console.WriteLine(serverResponse);//The server answer is ouputed
                                response = serverResponse;
                            }
                            else//When user's locations found
                            {
                                Console.WriteLine(userName + " is " + locationInput);//The message shows where user is located
                                response = userName + " is " + locationInput;
                            }
                        }
                        // if client is wanting to update location, do this:
                        else
                        {
                            string combineString = $"name={userName}&location={locationInput}";//part of request string
                            int combineStringLength = combineString.Length;//Lenght of string above
                            sw.WriteLine($"POST / HTTP/1.1\r\nHost: {serverName}\r\nContent-Length: {combineStringLength}\r\n\r\n" + combineString);//Request to server for current update protocol
                            sw.Flush();
                            string serverResponse = sr.ReadToEnd();//When server send respond this variable is reading answer till end
                            string[] sections = serverResponse.Split(new char[] { ' ' }, 3);//This line is splitting the respond by spaces
                            serverResponse = sections[2];//Part of server respond needed
                            if (serverResponse == sections[2])//If is "OK"
                            {

                                Console.WriteLine(userName + " location changed to be " + locationInput);//Print where user's locating has been chnaged
                                response = userName + " location changed to be " + locationInput;
                            }
                            else
                            {
                                Console.WriteLine(serverResponse);//Print server respond 
                                response = serverResponse;
                            }
                        }
                        break;
                    //The "-h0" protocol is used for HTTP/1.0 protocol
                    case "-h0":
                        // if the client wants to do look up
                        if (locationInput == null)// if location input is null 
                        {
                            sw.WriteLine($"GET /?{userName} HTTP/1.0\r\n");//Request to server for current look up protocol
                            sw.Flush();
                            string serverResponse = sr.ReadToEnd();
                            string[] sections = serverResponse.Split(new string[] { "\r\n" }, StringSplitOptions.None);//This line is splitting the respond by lines
                            locationInput = sections[3];//last line is the acctual location variable 

                            if (serverResponse == "HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n")
                            {

                                Console.WriteLine(serverResponse);
                                response = serverResponse;
                            }
                            else
                            {
                                Console.WriteLine(userName + " is " + locationInput);
                                response = userName + " is " + locationInput;
                            }
                        }

                        // if client is wanting to update location, do this:
                        else
                        {
                            int locLength = locationInput.Length;
                            sw.WriteLine($"POST /{userName} HTTP/1.0\r\nContent-Length: {locLength}\r\n\r\n{locationInput}");//Request to server for current update protocol
                            sw.Flush();
                            string serverResponse = sr.ReadLine();
                            string[] sections = serverResponse.Split(new char[] { ' ' }, 3);
                            serverResponse = sections[2];
                            if (serverResponse == sections[2])
                            {

                                Console.WriteLine(userName + " location changed to be " + locationInput);
                                response = userName + " location changed to be " + locationInput;
                            }
                            else
                            {
                                Console.WriteLine(serverResponse);
                                response = serverResponse;
                            }
                        }
                        break;
                    //The "-h9" protocol is used for HTTP/0.9 protocol
                    case "-h9":
                        // if the client wants to do look up
                        if (locationInput == null)// if there is 1 arg 
                        {
                            sw.WriteLine("GET /" + userName);//Request to server for current look up protocol
                            sw.Flush();
                            string serverResponse = sr.ReadToEnd();
                            string[] sections = serverResponse.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                            locationInput = sections[3];

                            if (serverResponse == "HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n")
                            {

                                Console.WriteLine(serverResponse);
                                response = serverResponse;
                            }
                            else
                            {
                                Console.WriteLine(userName + " is " + locationInput);
                                response = userName + " is " + locationInput;
                            }
                        }
                        // if client is wanting to update location, do this:
                        else
                        {
                            sw.WriteLine("PUT /" + userName + "\r\n\r\n" + locationInput);//Request to server for current update protocol
                            sw.Flush();

                            string serverResponse = sr.ReadLine();
                            string[] sections = serverResponse.Split(new char[] { ' ' }, 3);
                            serverResponse = sections[2];
                            if (serverResponse == sections[2])
                            {

                                Console.WriteLine(userName + " location changed to be " + locationInput);
                                response = userName + " location changed to be " + locationInput;
                            }
                            else
                            {
                                Console.WriteLine(serverResponse);
                                response = serverResponse;
                            }
                        }
                        break;
                    //This is standart "whois" protocol if none of the protocol has been chosen
                    case "whois":
                        // if the client wants to do look up
                        if (locationInput == null)// if there is 1 arg 
                        {
                            sw.WriteLine(userName);
                            sw.Flush();
                            string serverResponse = sr.ReadToEnd();

                            if (serverResponse == "ERROR: no entries found\r\n")
                            {
                                Console.WriteLine(serverResponse);
                                response = serverResponse;
                            }
                            else
                            {
                                Console.WriteLine(userName + " is " + serverResponse);
                                response = userName + " is " + serverResponse;
                            }
                        }

                        // if client is wanting to update location, do this:
                        else
                        {
                            sw.WriteLine(userName + " " + locationInput);
                            sw.Flush();

                            string serverResponse = sr.ReadToEnd();
                            if (serverResponse == "OK\r\n")
                            {
                                Console.WriteLine(userName + " location changed to be " + locationInput);
                                response = userName + " location changed to be " + locationInput;
                            }
                            else
                            {
                                Console.WriteLine("Error");
                                response = "Error";
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Protocol not known");
                        break;

                }
            }
            //Error handling for protocol switch of is fails
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }
    }
}