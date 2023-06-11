using WatsonWebserver;
//using static System.Net.WebRequestMethods;
using System.IO;
using System.Runtime.InteropServices;
using BCrypt.Net;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace server
{
    /// <summary>
    /// this class is to keep everything that has to do with the web server in.
    /// right now it is a simple web server hosting files from the html directory.
    /// </summary>
    class WebServer
    {
        /// <summary>
        /// the server object.
        /// </summary>
        private Server webserver;

        public WebServer(List<string> hostNames, int port)
        {
            webserver = new Server(hostNames, port, false);
            // set the directory where all the files are
            webserver.Routes.Content.BaseDirectory = "./html/";
            // add all the files from the above base directory.
            webserver.Routes.Content.Add("/", true);
            // add the preRouting this is where we added code to look for index files.
            webserver.Routes.PreRouting = PreRoutingHandler;
        }
        /// <summary>
        /// Start the web server.
        /// </summary>
        public void start()
        {
            webserver.Start();
        }

        /// <summary>
        /// look for index files if a directory is requested
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        static async Task<bool> PreRoutingHandler(HttpContext ctx)
        {
            //string urlfull = ctx.Request.Url.RawWithQuery;
            Dictionary<string, string> variables = new Dictionary<string, string>();
            variables.Add("title", "Dark Days");
            // log a user in.
            if (ctx.Request.Url.RawWithQuery == "/" && ctx.Request.Method == WatsonWebserver.HttpMethod.POST)
            {
                // this means we are posting a log in
                string data = ctx.Request.DataAsString;
                if (data.Contains("username") && data.Contains("password"))
                {
                    Dictionary<string, string> loginData = GetUserAndPass(data);
                    try
                    {
                        User? loggedInUser = UserSystem.Login(loginData["username"], loginData["password"]);
                        string sessionId = UserSystem.AssignSessionId(loggedInUser);
                        ctx.Response.Headers.Add("Set-Cookie", $"sessionId={sessionId}; Max-Age=86400;");
                        //await ctx.Response.Send(GetRedirect("/game.html")); 
                        //return true;
                        ctx.Request.Method = WatsonWebserver.HttpMethod.GET;
                        ctx.Request.Url.RawWithQuery = ctx.Request.Url.RawWithQuery + "game.html";
                        //return false;
                    }
                    catch (Exception ex)
                    {
                        //await ctx.Response.Send(ex.Message);
                        //return true;
                        variables.Add("name", loginData["username"]);
                        variables.Add("error", ex.Message);
                    }
                }
            }
            // register a new user.
            else if (ctx.Request.Url.RawWithQuery.StartsWith("/register") && ctx.Request.Method == WatsonWebserver.HttpMethod.POST)
            {
                string data = ctx.Request.DataAsString;
                Dictionary<string, string> loginData = GetUserAndPass(data);
                variables.Add("name", loginData["username"]);
                variables.Add("password", loginData["password"]);
                try
                {
                    UserSystem.CreateNewUser(loginData["username"], loginData["password"]);
                    ctx.Request.Method = WatsonWebserver.HttpMethod.GET;
                    ctx.Request.Url.RawWithQuery = "/";
                    
                    variables.Add("error", "User Registered!");
                    //return false;
                }
                catch (Exception ex)
                {
                    ctx.Request.Method = WatsonWebserver.HttpMethod.GET;
                    variables.Add("error", ex.Message);
                    //ctx.Request.Url.RawWithQuery = "/";
                    //return true;
                }
            }
            FileAttributes attr = File.GetAttributes("./html" + ctx.Request.Url.RawWithQuery);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                if (!ctx.Request.Url.RawWithQuery.EndsWith("/"))
                {
                    ctx.Request.Url.RawWithQuery += "/";
                }
                if (File.Exists("./html" + ctx.Request.Url.RawWithQuery + "index.html"))
                {
                    ctx.Request.Url.RawWithQuery = ctx.Request.Url.RawWithQuery + "index.html";
                }
                else if (File.Exists("./html" + ctx.Request.Url.RawWithQuery + "index.htm"))
                {
                    ctx.Request.Url.RawWithQuery = ctx.Request.Url.RawWithQuery + "index.htm";
                }
            }
            // run server side edits to any html.
            return await PreprocessPage(ctx, variables);
        }

        private static async Task<bool> PreprocessPage(HttpContext ctx, Dictionary<string, string> variables)
        {
            // run server side edits to any html.
            if (ctx.Request.Url.RawWithQuery.EndsWith(".html") && File.Exists("./html" + ctx.Request.Url.RawWithQuery))
            {
                string data = File.ReadAllText("./html" + ctx.Request.Url.RawWithQuery);
                data = ReplaceVariables(data, variables);
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                Stream s = new MemoryStream(bytes);
                ctx.Response.StatusCode = 200;
                ctx.Response.ChunkedTransfer = true;
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int bytesRead = await s.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        await ctx.Response.SendChunk(buffer, bytesRead);
                    }
                    else
                    {
                        await ctx.Response.SendFinalChunk(null, 0);
                        break;
                    }
                }
                return true;
            }

            return false;  // allow the connection
        }


        private static string ReplaceVariables(string pageString, Dictionary<string,string> variables)
        {
            foreach (KeyValuePair<string,string> variable in variables)
            {
                pageString = Regex.Replace(pageString, $"<%=\\s*{variable.Key}\\s*%>", variable.Value);
            }
            return Regex.Replace(pageString, "<%=.*?%>", "");
        }

        private static byte[] GetRedirect(string url)
        {
            string returnHtml = "<!DOCTYPE html>" +
                            "<html lang=\"en\" >" +
                            "<head>" +
                            "<meta charset=\"utf-8\">" +
                            "<title> Having Fun </title>" +
                            $"<meta http-equiv=\"Refresh\" content=\"0; url='{url}'\" />"+
                            "</head>" +
                            "<body>" +
                            "</body>" +
                            "</html>";
            return Encoding.ASCII.GetBytes(returnHtml);

        }

        private static Dictionary<string, string> GetUserAndPass(string data)
        {
            Dictionary<string, string> returnKvp = new Dictionary<string, string>();
            foreach (string keyValuePairs in data.Split('&'))
            {
                string[] kvp = keyValuePairs.Split('=');
                if (kvp.Length == 2)
                {
                    returnKvp.Add(kvp[0], kvp[1]);
                }
            }
            return returnKvp;
        }


        /// <summary>
        /// This is not used but could be used to do some default routing 
        /// when not found...
        /// might want this to send user back to login page at some point.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        static async Task DefaultRoute(HttpContext ctx)
        {
            await ctx.Response.Send("Hello from the default route!");
        }

        /// <summary>
        /// this was a test to have a file sent back in response.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        static async Task DownloadChunkedFile(HttpContext ctx)
        {
            using (FileStream fs = new FileStream("./html/index.html", FileMode.Open, FileAccess.Read))
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ChunkedTransfer = true;
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        await ctx.Response.SendChunk(buffer, bytesRead);
                    }
                    else
                    {
                        await ctx.Response.SendFinalChunk(null, 0);
                        break;
                    }
                }
            }
            return;
        }


        public static string? GetIpAddress()
        {
            string? localIP = null;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    localIP = endPoint.Address.ToString();
                }
            }
            return localIP;
        }
    }
}

