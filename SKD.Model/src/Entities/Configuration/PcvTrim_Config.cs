namespace SKD.Model;

public class PcvTrim_Config : IEntityTypeConfiguration<PcvTrim> {
    public void Configure(EntityTypeBuilder<PcvTrim> builder) {

        builder.ToTable("pcv_trim");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Pcvs)
            .WithOne(t => t.PcvTrim)
            .HasForeignKey(t => t.PcvTrimId);

    }
}