using System;
using Discord;
using Discord.Commands;

namespace EmissaryCore
{
    public class EmissaryResult : RuntimeResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public EmissaryResult(CommandError? error, string reason) 
            : base(error, reason)
        {
        }

        public static EmissaryResult FromSuccess(string message)
        {
            EmissaryResult result = new EmissaryResult(null, message);
            result.Message = message;
            result.Success = true;
            result.ErrorMessage = "";
            return result;
        }

        public static EmissaryResult FromError(string reason)
        {
            EmissaryResult result = new EmissaryResult(CommandError.Unsuccessful, reason);
            result.Message = "";
            result.Success = false;
            result.ErrorMessage = reason;
            return result;
        }
    }
}