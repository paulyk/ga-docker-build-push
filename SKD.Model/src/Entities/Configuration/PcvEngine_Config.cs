namespace SKD.Model;

public class PcvEngine_Config : IEntityTypeConfiguration<PcvEngine> {
    public void Configure(EntityTypeBuilder<PcvEngine> builder) {

        builder.ToTable("pcv_engine");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.PcvEngine)
            .HasForeignKey(t => t.PcvEngineId);

    }
}