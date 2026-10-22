namespace HotelAPI.DTOs;

public class CreateProductReservationDTO
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
}
