namespace SKD.Model;

public class PcvModel_Config : IEntityTypeConfiguration<PcvModel> {
    public void Configure(EntityTypeBuilder<PcvModel> builder) {

        builder.ToTable("pcv_model");
        
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(EntityFieldLen.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Code).HasMaxLength(EntityFieldLen.Category_Code);
        builder.Property(t => t.Name).HasMaxLength(EntityFieldLen.Category_Name);

        builder.HasIndex(t => t.Code).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();
    }
}