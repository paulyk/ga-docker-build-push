namespace SKD.Model;

public class ComponentStation_Config : IEntityTypeConfiguration<ComponentStation> {
    public void Configure(EntityTypeBuilder<ComponentStation> builder) {

        builder.ToTable("component_station");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();

        builder.HasIndex(t => new { t.ComponentId, t.StationId }).IsUnique();
    }
}