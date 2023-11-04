namespace DataTransferTool.Models
{
    public class Selectfields
    {
        public string SourceTable { get; set; }
        public string CommonfieldSourceTable { get; set; }
        public string TargetfieldSourceTable { get; set; }
        public string DestinationTable { get; set; }
        public string CommonfieldDestinationTable { get; set; }
        public string TargetfieldDestinationTable { get; set; }
        public string SourceConnectionString { get; set; }
        public string DestinationConnectionString { get; set; }
    }
}
