using RestaurantReservation.Application.Interfaces.Repositories;

namespace RestaurantReservation.Application.Common.Helpers;

public class TableCodeGenerator()
{
    public static string Generate(string tableTypeName, int existingCount)
    {
        var prefix = tableTypeName.Length > 4
            ? tableTypeName.Substring(0, 4).ToUpper()
            : tableTypeName.ToUpper();

        return $"{prefix}{(existingCount + 1):D2}";
    }
}