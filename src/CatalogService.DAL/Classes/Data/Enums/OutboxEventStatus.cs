namespace CatalogService.DAL.Classes.Data.Enums
{
    public enum OutboxEventStatus
    {
        Pending = 0,
        Processing = 1,
        Succeeded = 2,
        Failed = 3,
        Abandoned = 4
    }
}
