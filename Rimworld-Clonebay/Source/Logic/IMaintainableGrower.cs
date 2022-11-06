namespace Clonebay
{
    /// <summary>
    /// Makes a Grower be able to be maintained.
    /// </summary>
    public interface IMaintainableGrower
    {
        float ScientistMaintence { get; set; }
        float DoctorMaintence { get; set; }
        float RoomCleanliness { get; }
    }
}
