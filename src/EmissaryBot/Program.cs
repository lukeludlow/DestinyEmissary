using System.Threading.Tasks;

namespace EmissaryBot
{
    class Program
    {
        public static Task Main(string[] args)
        {
            // if your application throws any exceptions within an async context, 
            // they will be thrown all the way back up to the first non-async method; 
            // since our first non-async method is the program's Main method, 
            // this means that all unhandled exceptions will be thrown up there, which will crash the application.
            return Startup.MainAsync();
        }
    }
}
