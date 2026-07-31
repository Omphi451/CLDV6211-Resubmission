using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventEase2.Models.ViewModels
{
    public class SearchViewModel
    {
        public List<BookingViewModel> Bookings { get; set; } = new List<BookingViewModel>();
        public string? SearchTerm { get; set; }
        public int? EventTypeId { get; set; }
        public int? VenueId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<SelectListItem> EventTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Venues { get; set; } = new List<SelectListItem>();
    }
}