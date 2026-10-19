namespace CDCTarget.Modules
{
    public class ThroughputDetector
    {
        private int _eventCount = 0;
        private DateTime _lastSpikeCheck = DateTime.Now;

        public void RecordEventSpike()
        {
            _eventCount++;
            var timeSinceLastCheck = DateTime.Now - _lastSpikeCheck;

            // Log throughput every 5 seconds
            if (timeSinceLastCheck.TotalSeconds >= 5)
            {
                var tps = _eventCount / timeSinceLastCheck.TotalSeconds;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[MIGRATION DASHBOARD] Throughput: {tps:F2} Events/sec (Total: {_eventCount})");
                Console.ResetColor();

                _eventCount = 0;
                _lastSpikeCheck = DateTime.Now;
            }
        }
    }
}