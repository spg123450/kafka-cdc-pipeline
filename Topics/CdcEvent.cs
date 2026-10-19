namespace Topics
{
    public class CdcEvent
    {
        // "insert", "update", "delete", etc.
        public string OperationType { get; set; }

        public CdcDocumentKey DocumentKey { get; set; }
        public CdcUpdateDescription UpdateDescription { get; set; }

        // This holds the actual database row payload as a raw JSON string
        public string FullDocument { get; set; }
    }

    public class CdcDocumentKey
    {
        public string _id { get; set; }
    }

    public class CdcUpdateDescription
    {
        // We use object because updated fields are dynamic based on what changed
        public object UpdatedFields { get; set; }
        public string[] RemovedFields { get; set; }
    }
}