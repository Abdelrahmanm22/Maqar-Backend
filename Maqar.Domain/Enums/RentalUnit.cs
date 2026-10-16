namespace Maqar.Domain.Enums
{
    /// <summary>What exactly is rented. Set only when the listing is for rent; a Room or Bed listing's specs describe that room, not the whole unit.</summary>
    public enum RentalUnit
    {
        Whole = 1,
        Room = 2,
        Bed = 3
    }
}
