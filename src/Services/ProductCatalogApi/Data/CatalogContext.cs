using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogApi.Domain;

namespace ProductCatalogApi.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CatalogBrand>(ConfigureCatalogBrand);
            builder.Entity<CatalogType>(ConfigureCatalogType);
            builder.Entity<CatalogItem>(ConfigureCatalogItem);
        }

        /* To begin with, a little info about HiLo Pattern. 
         * HiLo is a pattern where the primary key is made of 2 parts “Hi” and “Lo”. Where the “Hi” part comes from database and “Lo” 
         * part is generated in memory to create unique value. Remember, “Lo” is a range number like 0-100. So when “Lo” 
         * range is exhausted for “Hi” number, then again a database call is made to get next “Hi number”. 
         * So the advantage of HiLo pattern is that you know the key value in advance. Let’s see how to use 
         * HiLo to generate keys with Entity Framework Core.
         
         * As you can see it starts with 1 and get increment by 10. 
         * There is a difference between a Sequence and HiLo Sequence with respect to INCREMENT BY option. 
         * In Sequence, INCREMENT BY will add “increment by” value to previous sequence value to generate new value. 
         * So in this case, if your previous sequence value was 11, then next sequence value would be 11+10 = 21. 
         * And in case of HiLo Sequence, INCREMENT BY option denotes a block value which means that next sequence 
         * value will be fetched after first 10 values are used.
         * 
         * https://www.talkingdotnet.com/use-hilo-to-generate-keys-with-entity-framework-core/
         */


        private void ConfigureCatalogItem(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("Catalog");
            builder.Property(c => c.Id)
                .ForSqlServerUseSequenceHiLo("catalog_hilo")
                .IsRequired(true);

            builder.Property(c => c.Name)
                    .IsRequired(true)
                    .HasMaxLength(50);

            builder.Property(c => c.Price)
                .IsRequired(true);

            builder.Property(c => c.PictureUrl)
                .IsRequired(false);

            builder.HasOne(c => c.CatalogBrand)
                .WithMany()
                .HasForeignKey(c => c.CatalogBrandId);

            builder.HasOne(c => c.CatalogType)
               .WithMany()
               .HasForeignKey(c => c.CatalogTypeId);

        }

        private void ConfigureCatalogType(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogType");

            builder.Property(c => c.id)
                .ForSqlServerUseSequenceHiLo("catalog_type_hilo")
                .IsRequired();

            builder.Property(c => c.Type)
                .IsRequired()
                .HasMaxLength(100);
        }

        private void ConfigureCatalogBrand(EntityTypeBuilder<CatalogBrand> builder)
        {
            builder.ToTable("CatalogBrand");

            builder.Property(c => c.id)
                .ForSqlServerUseSequenceHiLo("catalog_brand_hilo")
                .IsRequired();

            builder.Property(c => c.Brand)
                .IsRequired()
                .HasMaxLength(100);
        }

        public DbSet<CatalogType> CatalogTypes { get; set; }
        public DbSet<CatalogBrand> CatalogBrands { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }
    }
}
