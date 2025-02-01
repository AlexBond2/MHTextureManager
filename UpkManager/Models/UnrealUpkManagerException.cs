using System;

namespace UpkManager.Models
{
    public class UnrealUpkManagerException
    {
        public Exception Exception { get; set; }
        public string MachineName { get; set; }
        public DateTime HappenedAt { get; set; }
    }
}
