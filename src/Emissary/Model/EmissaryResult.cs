using System;
using Discord;
using Discord.Commands;

namespace EmissaryCore
{
    public class EmissaryResult : RuntimeResult
    {
        public string SuccessMessage { get; set; }
        public bool ErrorOccurred { get; set; }
        public string ErrorMessage { get; set; }

        public EmissaryResult(CommandError? error, string reason) 
            : base(error, reason)
        {
        }
    }
}