using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string TypeName { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Event>? Events { get; set; }
    }
}