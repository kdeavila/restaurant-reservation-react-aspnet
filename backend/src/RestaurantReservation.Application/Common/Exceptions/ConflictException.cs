namespace RestaurantReservation.Application.Common.Exceptions;

public class ConflictException(string message) : Exception(message);