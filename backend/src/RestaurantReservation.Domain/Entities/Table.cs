using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Domain.Entities;

public class Table
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
    public int TableTypeId { get; set; }
    public TableStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public TableType TableType { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}