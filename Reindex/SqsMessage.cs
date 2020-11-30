namespace Reindex
{
    public class SqsMessage
    {
        public string taskId { get; set; }
        public string newIndex { get; set; }
        public string alias { get; set; }
        public bool deleteAfterReindex { get; set; }
    }
}
