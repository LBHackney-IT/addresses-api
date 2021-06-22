using System;
using System.Collections.Generic;
using System.Text;
using Reindex;

namespace ReindexTests.MessageQueue
{
    public class HandlerTestDouble : Handler
    {
        public string LastError { get; set; }

        public HandlerTestDouble()
        {
            // Don't call base constructor so no dependencies are created.
        }

        protected override void Log(string message)
        {
            LastError = message;
        }
    }
}
