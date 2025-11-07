using CatalogService.DAL.Classes.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.DAL.Classes.Data.Entities
{
    public class OutboxEvent
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string EventType { get; set; } = default!;

        [Required]
        public DateTime OccurredOnUtc { get; set; }

        [Required]
        public string Payload { get; set; } = default!; // JSON serialized integration event

        [Required]
        [MaxLength(200)]
        public string RoutingKey { get; set; } = default!;

        public int Version { get; set; }

        public int Attempts { get; set; }

        public OutboxEventStatus Status { get; set; }

        public DateTime? ProcessedOnUtc { get; set; }

        [MaxLength(1000)]
        public string? LastError { get; set; }

        // Optional correlation
        [MaxLength(100)]
        public string? CorrelationId { get; set; }
    }
}
