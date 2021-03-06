namespace Reindex
{
    public class ReindexRequest
    {
        /// <summary>
        /// Required.
        /// </summary>
        public string alias { get; set; }
        /// <summary>
        /// Optional request parameter. If not supplied it use the index attached to the alias as the reindex source.
        /// </summary>
        public string fromIndex { get; set; }
        /// <summary>
        /// Optional request parameter. If not supplied it will use the config saved in the data folder of this repository.
        /// </summary>
        public string config { get; set; } = null;

        /// <summary>
        /// Optional request parameter. If supplied after reindexing, any indexes previously linked to the alias will
        /// be deleted. Defaults to false.
        /// </summary>
        public bool deleteAfterReindex { get; set; } = false;
    }
}
