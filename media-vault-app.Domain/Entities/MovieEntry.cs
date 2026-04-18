namespace media_vault_app.Domain.Entities
{
    public sealed record MovieEntry : MediaEntry
    {
        public int RuntimeMinutes { get; set; }
    }
}
