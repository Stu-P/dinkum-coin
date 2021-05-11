using System;
namespace DinkumCoin.Mining.Core.Models
{
    public class MiningTask
    {
        public int Id { get; set; }
        public MiningTaskStatus Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public long BlockId { get; set; }
        public string Solution { get; set; }
    }
}
