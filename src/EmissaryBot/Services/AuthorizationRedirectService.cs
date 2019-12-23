using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Discord;

namespace EmissaryBot
{
    /// <summary>
    /// i expose a public secure https address through my azure cloud vm,
    /// then i have an nginx reverse proxy that redirects the request to a localhost httplistener 
    /// </summary>
    public class AuthorizationRedirectService
    {
        private readonly HttpListener httpListener;
        private readonly LogService log;
        private string pageData;

        public AuthorizationRedirectService(HttpListener httpListener, LogService log)
        {
            this.httpListener = httpListener;
            this.log = log;
            this.pageData = "<!DOCTYPE><html><body><p>success! you can close this window and return to discord</p></body></html>";
        }

        public async Task StartAsync()
        {
            string listenerPrefix = "http://localhost:6969/";
            await log.LogAsync(new LogMessage(LogSeverity.Info, "HttpListenerService", $"starting listener at {listenerPrefix}"));
            httpListener.Start();
            httpListener.Prefixes.Add(listenerPrefix);
            await HandleIncomingConnections(httpListener);
            httpListener.Close();
        }


        public async Task HandleIncomingConnections(HttpListener httpListener)
        {
            bool runServer = true;
            while (runServer) {
                HttpListenerContext context = await httpListener.GetContextAsync();
                LogRequestInfo(context.Request);
                WriteResponse(context.Response);

                // authorization url to send to user:
                // https://www.bungie.net/en/OAuth/Authorize?client_id=30910&response_type=code

                // client_id = "30910";
                // state = discordUser.Id;
                // GET https://www.bungie.net/en/oauth/authorize?client_id=30910&response_type=code&state=221313820847636491

                // once i've received the code,
                // "sending authentication code to emissary authentication service"
                // then Emissary handles all the rest (POST request and storing tokens and stuff)

                await log.LogAsync(new LogMessage(LogSeverity.Verbose, "HttpListenerService", "TODO passing authorization code to emissary auth service"));

                // TODO read the state parameter!!! i will set the state param to be the user's discord id
                // request.QueryString["code"]
                // request.QueryString["state"]  (the user's discord id)

            }
        }

        private async void LogRequestInfo(HttpListenerRequest request)
        {
            await log.LogAsync(new LogMessage(LogSeverity.Verbose, "HttpListenerService", "received request"));
            await log.LogAsync(new LogMessage(LogSeverity.Verbose, "HttpListenerService", $"url: {request.Url.OriginalString}"));
            await log.LogAsync(new LogMessage(LogSeverity.Verbose, "HttpListenerService", $"Referer: {request.Headers["Referer"]}"));
            await log.LogAsync(new LogMessage(LogSeverity.Verbose, "HttpListenerService", $"User-Agent: {request.Headers["User-Agent"]}"));
            string queryParams = string.Join(" ", request.QueryString.AllKeys.Select(key => $"{key}: {request.QueryString[key]}"));
            await log.LogAsync(new LogMessage(LogSeverity.Verbose, "HttpListenerService", $"query: {queryParams}"));
        }

        private async void WriteResponse(HttpListenerResponse response)
        {
            byte[] data = Encoding.UTF8.GetBytes($"{pageData}");
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }

    }
}