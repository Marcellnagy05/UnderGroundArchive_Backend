using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UnderGroundArchive_Backend.Models;
using UnderGroundArchive_Backend.Services;

namespace UnderGroundArchive_Backend.Dbcontext
{
    public class UGA_DBContext : IdentityDbContext<ApplicationUser>
    {
        public UGA_DBContext(DbContextOptions<UGA_DBContext> options) : base(options) { }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Guest> Guest { get; set; }

        public DbSet<Requests> Requests { get; set; }

        public DbSet<Categories> Categories { get; set; }

        public DbSet<Books> Books { get; set; }

        public DbSet<Achievements> Achievements { get; set; }

        public DbSet<CompletedAchievements> CompletedAchievements { get; set; }

        public DbSet<Genre> Genre { get; set; }

        public DbSet<Ranks> Ranks { get; set; }

        public DbSet<Comments> Comments { get; set; }

        public DbSet<Subscription> Subscription { get; set; }

        public DbSet<ReaderRatings> ReaderRatings { get; set; }

        public DbSet<CriticRatings> CriticRatings { get; set; }

        public DbSet<Reports> Reports { get; set; }

        public DbSet<ReportTypes> ReportTypes { get; set; }
        public DbSet<Chapters> Chapters { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Favourites> Favourites { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categories>(entity =>
            {
                modelBuilder.Entity<Categories>()
                    .Property(c => c.CategoryName)
                    .HasColumnType("varchar(255)");

                modelBuilder.Entity<Categories>()
                    .Property(c => c.IsAgeRestricted)
                    .HasColumnType("TINYINT(1)");
            });

            // Primary Keys
            modelBuilder.Entity<ApplicationUser>().HasKey(u => u.Id);
            modelBuilder.Entity<Guest>().HasKey(g => g.GuestId);
            modelBuilder.Entity<Requests>().HasKey(r => r.RequestId);
            modelBuilder.Entity<Categories>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<Books>().HasKey(b => b.BookId);
            modelBuilder.Entity<Achievements>().HasKey(a => a.AchievementId);
            modelBuilder.Entity<CompletedAchievements>().HasKey(ca => ca.Id);
            modelBuilder.Entity<Genre>().HasKey(g => g.GenreId);
            modelBuilder.Entity<Ranks>().HasKey(r => r.RankId);
            modelBuilder.Entity<Comments>().HasKey(c => c.CommentId);
            modelBuilder.Entity<Subscription>().HasKey(s => s.SubscriptionId);
            modelBuilder.Entity<ReaderRatings>().HasKey(rr => new { rr.RatingId });
            modelBuilder.Entity<CriticRatings>().HasKey(cr => new { cr.RatingId });
            modelBuilder.Entity<Reports>().HasKey(c => c.ReportId);
            modelBuilder.Entity<ReportTypes>().HasKey(s => s.ReportTypeId);

            modelBuilder.Entity<Chapters>().HasKey(ch => ch.ChapterId);
            modelBuilder.Entity<Favourites>().HasKey(f => f.FavouriteId);

            // Foreign Keys

            // Books Table
            modelBuilder.Entity<Books>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId);

            modelBuilder.Entity<Books>()
                .HasOne(b => b.Categories)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Books>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(b => b.AuthorId);

            // Requests Table
            modelBuilder.Entity<Requests>()
                .HasOne(r => r.Users)
                .WithMany(u => u.Requests)
                .HasForeignKey(r => r.RequesterId);

            // Achievements Table
            modelBuilder.Entity<Achievements>()
                .HasOne(a => a.Books)
                .WithMany(b => b.Achievements)
                .HasForeignKey(a => a.BookId);

            // CompletedAchievements Table
            modelBuilder.Entity<CompletedAchievements>()
                .HasOne(ca => ca.Achievements)
                .WithMany(a => a.CompletedAchievements)
                .HasForeignKey(ca => ca.AchievementId);

            modelBuilder.Entity<CompletedAchievements>()
                .HasOne(ca => ca.Users)
                .WithMany(u => u.CompletedAchievements)
                .HasForeignKey(ca => ca.CompleterId);

            // Comments Table
            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Books)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BookId);

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Users)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.CommenterId);

            // ReaderRatings Table
            modelBuilder.Entity<ReaderRatings>()
                .HasOne(rr => rr.Books)
                .WithMany(b => b.ReaderRatings)
                .HasForeignKey(rr => rr.BookId);

            // CriticRatings Table
            modelBuilder.Entity<CriticRatings>()
                .HasOne(cr => cr.Books)
                .WithMany(b => b.CriticRatings)
                .HasForeignKey(cr => cr.BookId);

            // Reports Table
            modelBuilder.Entity<Reports>()
                .HasOne(cr => cr.ReportTypes)
                .WithMany(b => b.Reports)
                .HasForeignKey(cr => cr.ReportTypeId);

            modelBuilder.Entity<Reports>()
                .HasOne(rp => rp.ReporterPeople)
                .WithMany(b => b.ReportSender)
                .HasForeignKey(cr => cr.ReporterId);

            modelBuilder.Entity<Reports>()
                .HasOne(rp => rp.ReportedPeople)
                .WithMany(b => b.ReportSubject)
                .HasForeignKey(cr => cr.ReportedPersonId);

            modelBuilder.Entity<Reports>()
                .HasOne(rp => rp.ReportedComments)
                .WithMany(b => b.ReportSubject)
                .HasForeignKey(cr => cr.ReportedCommentId);

            modelBuilder.Entity<Reports>()
                .HasOne(rp => rp.ReportedBooks)
                .WithMany(b => b.ReportSubject)
                .HasForeignKey(cr => cr.ReportedBookId);

            //Chapters table

            modelBuilder.Entity<Chapters>()
                .HasOne(b => b.Book)
                .WithMany(g => g.Chapters)
                .HasForeignKey(b => b.BookId);

            //Favourites table

            modelBuilder.Entity<Favourites>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favourites)
                .HasForeignKey(f => f.UserId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favourites>()
                .HasOne(f => f.Book)
                .WithMany(b => b.Favourites)
                .HasForeignKey(f => f.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favourites>()
                .HasOne(f => f.LastReadChapter)
                .WithMany(c => c.Favourites)
                .HasForeignKey(f => f.ChapterId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
