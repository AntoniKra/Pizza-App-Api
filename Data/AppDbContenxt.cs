using Microsoft.EntityFrameworkCore;
using PizzaApp.Entities;

namespace PizzaApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Pizzeria> Pizzerias { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Customer>("Customer")
                .HasValue<Owner>("Owner");

           modelBuilder.Entity<Promotion>()
                .HasMany(p => p.Pizzas)
                .WithMany(p => p.Promotions)
                .UsingEntity(j => j.ToTable("PromotionPizza"));

            modelBuilder.Entity<Pizza>()
                .Property(p => p.Style)
                .HasConversion<string>();

            modelBuilder.Entity<Pizza>()
                .Property(p => p.BaseSauce)
                .HasConversion<string>();

            modelBuilder.Entity<Pizza>()
                .Property(p => p.Dough)
                .HasConversion<string>();

            modelBuilder.Entity<Pizza>()
                .Property(p => p.Thickness)
                .HasConversion<string>();

            modelBuilder.Entity<Pizza>()
                .Property(p => p.Shape)
                .HasConversion<string>();

            modelBuilder.Entity<Promotion>()
                .Property(p => p.Type)
                .HasConversion<string>();

            // --- 1. SEEDING: DEFINICJA ID (Stałe GUID-y) ---

            // Kraje
            var polandId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35");
            var italyId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var usaId = Guid.Parse("20000000-0000-0000-0000-000000000002");
            var germanyId = Guid.Parse("30000000-0000-0000-0000-000000000003");

            // Miasta - Polska
            var warsawId = Guid.Parse("da2fd609-d754-4feb-8acd-c4f9ff13ba96");
            var krakowId = Guid.Parse("2902b665-1190-4c70-9915-b9c2d7680450");
            var gdanskId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var wroclawId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var poznanId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            // Miasta - Włochy 
            var romeId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var naplesId = Guid.Parse("88888888-8888-8888-8888-888888888888"); 
            var milanId = Guid.Parse("99999999-9999-9999-9999-999999999999");

            // Miasta - USA
            var newYorkId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var chicagoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            // Miasta - Niemcy
            var berlinId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            // --- 2. DODAWANIE KRAJÓW ---
            modelBuilder.Entity<Country>().HasData(
                new Country { Id = polandId, Name = "Polska", IsoCode = "PL", PhonePrefix = "+48" },
                new Country { Id = italyId, Name = "Włochy", IsoCode = "IT", PhonePrefix = "+39" },
                new Country { Id = usaId, Name = "Stany Zjednoczone", IsoCode = "US", PhonePrefix = "+1" },
                new Country { Id = germanyId, Name = "Niemcy", IsoCode = "DE", PhonePrefix = "+49" }
            );

            // --- 3. DODAWANIE MIAST ---
            modelBuilder.Entity<City>().HasData(
                // Polska
                new City { Id = warsawId, Name = "Warszawa", Region = "Mazowieckie", CountryId = polandId },
                new City { Id = krakowId, Name = "Kraków", Region = "Małopolskie", CountryId = polandId },
                new City { Id = gdanskId, Name = "Gdańsk", Region = "Pomorskie", CountryId = polandId },
                new City { Id = wroclawId, Name = "Wrocław", Region = "Dolnośląskie", CountryId = polandId },
                new City { Id = poznanId, Name = "Poznań", Region = "Wielkopolskie", CountryId = polandId },

                // Włochy
                new City { Id = romeId, Name = "Rzym", Region = "Lacjum", CountryId = italyId },
                new City { Id = naplesId, Name = "Neapol", Region = "Kampania", CountryId = italyId },
                new City { Id = milanId, Name = "Mediolan", Region = "Lombardia", CountryId = italyId },

                // USA
                new City { Id = newYorkId, Name = "Nowy Jork", Region = "Nowy Jork", CountryId = usaId },
                new City { Id = chicagoId, Name = "Chicago", Region = "Illinois", CountryId = usaId },

                // Niemcy
                new City { Id = berlinId, Name = "Berlin", Region = "Berlin", CountryId = germanyId }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}