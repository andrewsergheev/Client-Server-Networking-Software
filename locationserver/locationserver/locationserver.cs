using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Windows.Forms;


namespace locationserver
{
    /// <summary>
    /// This class is basiclly running the server either thru conlose or Windows Forms
    /// </summary>
    public class Responde
    {
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();//The dictionary is used as store function to store users and location
        //static string username = null;
        //static string location = null;

        public static int readTimeout = 1000;//default read timeout
        public static int writeTimeout = 1000;//defaulr write timeout

        public static Logging Log;//The logging function
        /// <summary>
        /// The Main method is procesing argument to either run the UI, server or addtional freature for the server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string filename = null;//this variable is for name of the file in the logging part
            bool UI = false;//this variable is for activating UI option
            for (int i = 0; i < args.Length; ++i)
            {
                //The switch above is processing options for the UI and loffing 
                switch (args[i])
                {
                    case "-w":
                        UI = true;//activate UI
                        break;
                    case "-l":
                        filename = args[++i];//logging feature 
                        break;
                    
                        
                    default:
                        Console.WriteLine("Unknown Option" + args[i]);
                        break;
                }

            }
            Log = new Logging(filename);

            //if (!args.Contains("-w"))
            if(UI == false)//When UI is off
            {
                
                runServer();//run the server in console
            }
            else//When UI is on
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new serverForm());//open the UI which is serverForm
            }
        }
        /// <summary>
        /// The function above is running the server which allow to client to connect to it
        /// </summary>
        public static void runServer()
        {
            TcpListener listener;
            Socket connection;
            Handler RequestHandler;
            
            try
            {
                listener = new TcpListener(IPAddress.Any, 43);//this is the way of the connection between client and server in port 43
                listener.Start();//Start of connectiom
                Console.WriteLine("Server started listening");
                while (true)//Thos loop allow to the server to be alway on for client requests
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler();
                    Thread t = new Thread(() => RequestHandler.doRequest(connection, Log));
                    t.Start();
                }
            }
            //error handling on server work
            catch (Exception e)
            {
                Console.WriteLine("Exeption: " + e.ToString());
            }
        }
        /// <summary>
        /// The method above is handling the server functionality
        /// </summary>
        class Handler
        {

            //static Dictionary<string, string> dictionary = new Dictionary<string, string>();
            static string username = null;
            static string location = null;
            /// <summary>
            /// The method above is processing reuests and protocol and send to the client answer base on what he requested
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="Log"></param>
            public void doRequest(Socket connection, Logging Log)
            {
                string host = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString();//this variable contains host info for the lodding function
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                Console.WriteLine("Cennection Recieved");
                string line = null;//This variable will contain request from client
                string status = "OK";//The status 
                try
                {
                    socketStream.ReadTimeout = readTimeout;//This is the read timeout for the server
                    socketStream.WriteTimeout = writeTimeout;//This is the write timeout for the server

                    StreamWriter sw = new StreamWriter(socketStream);//This function allows to write to client
                    StreamReader sr = new StreamReader(socketStream);//This function allows to read from client
                    line = sr.ReadLine();//This functions is reading the client request and set to the line variable
                    Console.WriteLine("Respond Recieved: " + line);//Printing the line

                    
                    //HTTP 1.1 request process
                    //HTTP 1.1 look up
                    if (line.StartsWith("GET /?name=") && line.EndsWith(" HTTP/1.1"))//looking for the specific request by startwith and ends with function 
                    {
                        string[] sections = line.Split(new char[] { ' ' });//This array is spliting the line by spaces
                        sr.ReadLine();//This function allows to read next line of the request
                        username = sections[1].Remove(0, 7);//assigning the username and removing not needed symbols from the protocol

                        if (dictionary.ContainsKey(username))//When username has a location
                        {
                            // the location can be found in the dictionary
                            location = dictionary[username];//look up for the locating assigned to a user
                            sw.WriteLine($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");//Print the server responde to a client
                            sw.Flush();//This function allows to send message to client
                        }
                        else //When location could not be found
                        {
                            // Oh no! This user is not known
                            sw.Write("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");//Print the server responde to a client
                            sw.Flush();//This function allows to send message to client
                        }

                    }
                    //HTTP 1.1 update
                    else if (line.Equals("POST / HTTP/1.1"))//looking for the specific request by ends with function 
                    {
                        sr.ReadLine();//This function allows to read next line of the request
                        sr.ReadLine();
                        sr.ReadLine();

                        string[] split = sr.ReadLine().Split(new char[] { '&' });//This array is spliting the line by & symbol
                        username = split[0].Remove(0, 5);//assigning the username and removing not needed symbols from the protocol
                        location = split[1].Remove(0, 9);//assigning the location and removing not needed symbols from the protocol


                        if (dictionary.ContainsKey(username))//When location exist for the user requested
                        {
                            dictionary[username] = location;//assign location
                        }
                        else//When location doesnt exist for the user
                        {
                            dictionary.Add(username, location);//add the location to the user
                        }
                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");//Print the server responde to a client
                        sw.Flush();//This function allows to send message to client
                    }

                    //HTTP 1.0 request process
                    //HTTP 1.0 look up
                    else if (line.StartsWith("GET /?") && line.EndsWith(" HTTP/1.0"))//looking for the specific request by startwith and ends with function 
                    {
                        string[] sections = line.Split(new char[] { ' ' });
                        username = sections[1].TrimStart('/', '?');//assigning the username and removing not needed symbols from the protocol request by using Trim functiom
                        if (dictionary.ContainsKey(username))//When username has a location
                        {
                            // the location can be found in the dictionary
                            location = dictionary[username];//look up for the locating assigned to a user
                            sw.WriteLine($"HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                            sw.Flush();

                        }
                        else
                        {
                            // Oh no! This user is not known
                            sw.Write("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }

                    }
                    //HTTP 1.0 update
                    else if (line.StartsWith("POST /") && line.EndsWith(" HTTP/1.0"))//looking for the specific request by startwith and ends with function 
                    {
                        string[] split = line.Split(new char[] { ' ' });
                        username = split[1].Remove(0, 1);//assigning the username and removing not needed symbols from the protocol
                        sr.ReadLine();
                        sr.ReadLine();
                        location = sr.ReadLine();//location is equl to the next line read


                        if (dictionary.ContainsKey(username))//When location exist for the user requested
                        {
                            dictionary[username] = location;
                        }
                        else//When location doesnt exist for the user
                        {
                            dictionary.Add(username, location);
                        }
                        sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        sw.Flush();
                    }

                    //HTTP 0.9
                    //HTTP 0.9 look up
                    else if (line.StartsWith("GET /"))//looking for the specific request by startwith and function 
                    {
                        string[] sections = line.Split(new char[] { '/' });
                        username = sections[1];
                        if (dictionary.ContainsKey(username))
                        {
                            // the location can be found in the dictionary
                            location = dictionary[username];
                            sw.WriteLine($"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                            sw.Flush();
                        }
                        else
                        {
                            // Oh no! This user is not known
                            sw.Write("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }

                    }
                    //HTTP 0.9 update
                    else if (line.StartsWith("PUT /"))//looking for the specific request by startwith and function 
                    {
                        string[] sections = line.Split(new char[] { '/' });
                        username = sections[1];
                        if (sr.Peek() >= 0)//when request has more than one line
                        {
                            //Do the HTTTP 0.9 update
                            sr.ReadLine();
                            location = sr.ReadLine();


                            if (dictionary.ContainsKey(username))
                            {
                                dictionary[username] = location;
                                sw.WriteLine($"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                                sw.Flush();
                            }
                            else
                            {
                                dictionary.Add(username, location);
                                sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                            }
                        }
                        else //When is just one line
                        {
                            //do the basic update
                            string[] chunk = line.Split(new char[] { ' ' }, 2);
                            username = chunk[0];
                            location = chunk[1];
                            if (dictionary.ContainsKey(username))
                            {
                                dictionary[username] = location;
                            }
                            else
                            {
                                dictionary.Add(username, location);
                            }
                            sw.WriteLine("OK");
                            sw.Flush();

                        }

                    }
                    //Basic look up and update
                    else
                    {
                        string[] sections = line.Split(new char[] { ' ' }, 2);

                        if (sections.Length == 1)
                        {
                            // We have a lookup
                            username = sections[0];
                            if (dictionary.ContainsKey(username))
                            {
                                // the location can be found in the dictionary
                                location = dictionary[username];
                            }
                            else
                            {
                                // Oh no! This user is not known
                                location = "ERROR: no entries found";
                            }
                            sw.WriteLine(location);
                            sw.Flush();
                        }
                        else if (sections.Length == 2)
                        {
                            // We have an update
                            username = sections[0];
                            location = sections[1];
                            if (dictionary.ContainsKey(username))
                            {
                                dictionary[username] = location;
                            }
                            else
                            {
                                dictionary.Add(username, location);
                            }
                            sw.WriteLine("OK");
                            sw.Flush();
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong as there are no words");
                            status = "UNKNOWN";
                        }
                    }


                }
                //Error handling for the protoc requests
                catch (Exception e)
                {

                    Console.WriteLine(e + " Something went wrong");
                    status = "Exceprion";
                }
                finally
                {
                    //When request is complete close in the socket as they are not needed any more
                    socketStream.Close();
                    connection.Close();

                    Log.WriteToLog(host, line, status);
                }
            }
        }
    }
    
}
/// <summary>
/// Loging system which is thredsafe method of writimg to a logfile
/// </summary>
public class Logging
{
    //The logfile is null because its gonna write to the console
    public static string LogFile = null;

    /// <summary>
    /// This creates logfile at specific name
    /// </summary>
    /// <param name="filename"></param>
    public Logging(string filename)
    {
        LogFile = filename; 
    }
    private static readonly object locker = new object();

    /// <summary>
    /// This writes a log entry to a console and optionally to the file 
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="message"></param>
    /// <param name="status"></param>
    public void WriteToLog (string hostname, string message, string status)
    {
        //Creates line in common log format 
        string line = hostname + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'") + " \"" + message + "\" " + status;
        //Lock the file write to prevent concurent threaded writes
        lock (locker)
        {
           
            Console.WriteLine(line);
            if (LogFile == null)
            {
                return;
            }
            //if no log file exist after writing to console 
            try
            {
                StreamWriter SW;
                SW = File.AppendText(LogFile);
                SW.WriteLine(line);
                SW.Close();
            }
            catch
            {
                //Write an error with cathcing the file
                Console.WriteLine("Unable to Write Log File " + LogFile); ;
            }
            
        }
    }
}