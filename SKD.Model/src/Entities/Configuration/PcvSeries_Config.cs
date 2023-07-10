namespace SKD.Model;

public class PcvSeries_Config : IEntityTypeConfiguration<PcvSeries> {
    public void Configure(EntityTypeBuilder<PcvSeries> builder) {

        builder.ToTable("pcv_series");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.PcvSeries)
            .HasForeignKey(t => t.PcvSeriesId);

    }
}