using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Reindex;

namespace ReindexTests.MessageQueue
{
    public class HandlerTestDouble : Handler
    {
        public string LastError { get; set; }

        /// <summary>
        /// Force use of the factory method so make this private
        /// This is used for unit tests, so we don't want the Handler constructor
        /// to be called as it instantiates dependencies that aren't available to unit tests
        /// </summary>
        private HandlerTestDouble()
        {
            // Don't call base constructor so no dependencies are created.
        }

        public static HandlerTestDouble HandlerTestDoubleFactory()
        {
            return (HandlerTestDouble) FormatterServices.GetUninitializedObject(typeof(HandlerTestDouble));
        }

        protected override void Log(string message)
        {
            LastError = message;
        }
    }
}
