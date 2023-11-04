namespace DataTransferTool.Models
{
    public class SharedModel
    {
        public string SourceTables { get; set; }
        public string DestinationTables { get; set; }
        public string SourceConnectionString { get; set; }
        public string DestinationConnectionString { get; set; }
    }
}
