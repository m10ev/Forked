using Forked.Models.Interfaces;

namespace Forked.Models.Domains
{
    public class Review : BaseEntity
    {
		public int Rating { get; set; }
		public string Message { get; set; }
    }
}
