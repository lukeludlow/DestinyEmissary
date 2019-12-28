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
        private readonly EmissaryService emissaryService;

        public AuthorizationRedirectService(HttpListener httpListener, LogService log, EmissaryService emissaryService)
        {
            this.httpListener = httpListener;
            this.log = log;
            this.pageData = "<!DOCTYPE><html><body><p>success! you can close this window and return to discord</p></body></html>";
            this.emissaryService = emissaryService;
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
                // by default, most browsers given a URL would make at least two calls. 
                // one call to the request URL and the other to favicon.ico.
                // so we need to ignore that second call for the favicon. 
                if (context.Request.Url.OriginalString.Contains("favicon.ico")) {
                    return;
                }
                LogRequestInfo(context.Request);

                // GET https://www.bungie.net/en/oauth/authorize?client_id=30910&response_type=code&state=221313820847636491

                await log.LogAsync(new LogMessage(LogSeverity.Info, "HttpListenerService", "passing authorization code to emissary auth service"));
                emissaryService.RegisterOrReauthorize(context.Request.QueryString["state"], context.Request.QueryString["code"]);

                WriteResponse(context.Response);
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