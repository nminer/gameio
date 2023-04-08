using WatsonWebserver;

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

    public WebServer(int port)
    {
        webserver = new Server("127.0.0.1", port, false);
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
        string urlfull = ctx.Request.Url.RawWithQuery;
        FileAttributes attr = File.GetAttributes("./html" + urlfull);

        if (attr.HasFlag(FileAttributes.Directory))
        {
            if (File.Exists("./html" + urlfull + "index.html"))
            {
                ctx.Request.Url.RawWithQuery = urlfull + "index.html";
            }
            else if (File.Exists("./html" + urlfull + "index.htm"))
            {
                ctx.Request.Url.RawWithQuery = urlfull + "index.htm";
            }
        }
        return false;  // allow the connection
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

}

