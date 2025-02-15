using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Songify.Controllers;
using Songify.Data;
using Songify.Entities;
using Songify.Models.SongModels;
using Songify.Models.LikedSongsModels;
using Songify.Models.BandModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Tests
{
    public class SongsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly SongsController _controller;
        public SongsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SongifyTestDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new SongsController(_context);
        }
        [Fact]
        public void All_ReturnsViewWithSongs()
        {
            var album = new Album { Id = 1, Title = "Test Album" };
            var band = new Band { Id = 1, Name = "Test Band" };
            _context.Albums.Add(album);
            _context.Bands.Add(band);
            _context.Songs.Add(new Song { Id = 1, Title = "Test Song", Duration = 180, AlbumId = album.Id, BandId = band.Id });
            _context.SaveChanges();
            var result = _controller.All(null) as ViewResult;
            var model = result.Model as List<SongAllViewModel>;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Test Song", model[0].Title);
        }

        [Fact]
        public void All_FiltersBySearchString()
        {
            var album = new Album { Id = 1, Title = "Album 1" };
            var band = new Band { Id = 1, Name = "Band 1" };
            _context.Albums.Add(album);
            _context.Bands.Add(band);
            _context.Songs.Add(new Song { Id = 2, Title = "Rock Song", Duration = 180, AlbumId = album.Id, BandId = band.Id });
            _context.Songs.Add(new Song { Id = 3, Title = "Pop Song", Duration = 200, AlbumId = album.Id, BandId = band.Id });
            _context.SaveChanges();
            var result = _controller.All("Rock") as ViewResult;
            var model = result.Model as List<SongAllViewModel>;
            Assert.NotNull(result);
            Assert.Single(model);
            Assert.Equal("Rock Song", model[0].Title);
        }

        [Fact]
        public void Create_Post_ValidData_RedirectsToAll()
        {
            var album = new Album { Id = 1, Title = "Album 1" };
            var band = new Band { Id = 1, Name = "Band 1" };
            _context.Albums.Add(album);
            _context.Bands.Add(band);
            _context.SaveChanges();

            var bindingModel = new SongCreateBindingModel
            {
                Title = "New Song",
                Duration = 250,
                AlbumId = album.Id,
                BandId = band.Id
            };

            var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin123"),
                new Claim(ClaimTypes.Role, "Admin")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = adminUser }
            };
            var result = _controller.Create(bindingModel) as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
        }

        [Fact]
        public void Create_Post_InvalidData_ReturnsView()
        {
            var bindingModel = new SongCreateBindingModel { Title = "", Duration = 0, AlbumId = 0, BandId = 0 };
            _controller.ModelState.AddModelError("Title", "Required");
            var result = _controller.Create(bindingModel) as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void My_ReturnsLikedSongs()
        {
            var userId = "user123";
            var user = new SongifyUser { Id = userId, UserName = "testuser" };
            var song = new Song { Id = 1, Title = "Liked Song", Duration = 180, AlbumId = 1, BandId = 1 };
            _context.Users.Add(user);
            _context.Songs.Add(song);
            _context.LikedSongs.Add(new LikedSong { UserId = userId, SongId = song.Id });
            _context.SaveChanges();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = userClaims }
            };
            var result = _controller.My(null) as ViewResult;
            var model = result.Model as List<LikedSongsListingViewModel>;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("1", model[0].SongId);
        }

        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithModel()
        {
            var album = new Album { Id = 1, Title = "Test Album" };
            var band = new Band { Id = 1, Name = "Test Band" };
            var song = new Song { Id = 2, Title = "Edit Song", Duration = 200, AlbumId = album.Id, BandId = band.Id };

            _context.Albums.Add(album);
            _context.Bands.Add(band);
            _context.Songs.Add(song);
            _context.SaveChanges();

            var result = _controller.Edit(2) as ViewResult;
            var model = result.Model as SongEditBindingModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Edit Song", model.Title);
        }

        [Fact]
        public void Edit_Get_InvalidId_ReturnsNotFound()
        {
            var result = _controller.Edit(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_ValidData_RedirectsToAll()
        {
            var album = new Album { Id = 1, Title = "Test Album" };
            var band = new Band { Id = 1, Name = "Test Band" };
            var song = new Song { Id = 3, Title = "Song to Edit", Duration = 150, AlbumId = album.Id, BandId = band.Id };
            _context.Albums.Add(album);
            _context.Bands.Add(band);
            _context.Songs.Add(song);
            _context.SaveChanges();

            var model = new SongEditBindingModel { Id = 3, Title = "Updated Song", Duration = 180, AlbumId = album.Id, BandId = band.Id };

            var result = _controller.Edit(model) as RedirectToActionResult;
            var updatedSong = _context.Songs.Find(3);

            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
            Assert.Equal("Updated Song", updatedSong.Title);
        }

        [Fact]
        public void Delete_Get_ValidId_ReturnsViewWithModel()
        {
            var song = new Song { Id = 4, Title = "Delete Song", Duration = 200, AlbumId = 1, BandId = 1 };
            _context.Songs.Add(song);
            _context.SaveChanges();

            var result = _controller.Delete(4) as ViewResult;
            var model = result.Model as SongDeleteViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Delete Song", model.Title);
        }

        [Fact]
        public void Delete_Get_InvalidId_ReturnsNotFound()
        {
            var result = _controller.Delete(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_ValidId_RedirectsToAll()
        {
            var song = new Song { Id = 5, Title = "To Be Deleted", Duration = 210, AlbumId = 1, BandId = 1 };
            _context.Songs.Add(song);
            _context.SaveChanges();

            var result = _controller.DeleteConfirmed(5) as RedirectToActionResult;
            var deletedSong = _context.Songs.Find(5);

            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
            Assert.Null(deletedSong);
        }

        [Fact]
        public void DeleteConfirmed_InvalidId_ReturnsNotFound()
        {
            var result = _controller.DeleteConfirmed(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
