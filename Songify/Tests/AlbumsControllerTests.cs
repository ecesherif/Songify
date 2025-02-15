using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Songify.Controllers;
using Songify.Data;
using Songify.Entities;
using Songify.Models.AlbumModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
namespace Tests
{
    public class AlbumsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AlbumsController _controller;

        public AlbumsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SongifyTestDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new AlbumsController(_context);
        }
        [Fact]
        public void All_ReturnsViewWithAlbums()
        {
            _context.Albums.Add(new Album { Id = 1, Title = "Test Album", ReleaseYear = 2022 });
            _context.SaveChanges();
            var result = _controller.All(null) as ViewResult;
            var model = result.Model as List<AlbumAllViewModel>;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Test Album", model[0].Title);
        }
        [Fact]
        public void All_FiltersBySearchString()
        {
            _context.Albums.Add(new Album { Id = 2, Title = "Rock Album", ReleaseYear = 2021 });
            _context.Albums.Add(new Album { Id = 3, Title = "Pop Album", ReleaseYear = 2023 });
            _context.SaveChanges();
            var result = _controller.All("Rock") as ViewResult;
            var model = result.Model as List<AlbumAllViewModel>;
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Rock Album", model[0].Title);
        }
        [Fact]
        public void Create_Post_ValidData_RedirectsToAll()
        {
            var bindingModel = new AlbumCreateBindingModel { Title = "New Album", ReleaseYear = 2024 };
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
            var bindingModel = new AlbumCreateBindingModel { Title = "", ReleaseYear = 0 };
            _controller.ModelState.AddModelError("Title", "Required");

            var result = _controller.Create(bindingModel) as ViewResult;

            Assert.NotNull(result);
        }
        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithModel()
        {
            var album = new Album { Id = 4, Title = "Edit Album", ReleaseYear = 2020 };
            _context.Albums.Add(album);
            _context.SaveChanges();

            var result = _controller.Edit(4) as ViewResult;
            var model = result.Model as AlbumEditBindingModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Edit Album", model.Title);
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
            var album = new Album { Id = 5, Title = "Old Title", ReleaseYear = 2019 };
            _context.Albums.Add(album);
            _context.SaveChanges();

            var model = new AlbumEditBindingModel { Id = 5, Title = "Updated Title", ReleaseYear = 2022 };

            var result = _controller.Edit(model) as RedirectToActionResult;
            var updatedAlbum = _context.Albums.Find(5);

            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
            Assert.Equal("Updated Title", updatedAlbum.Title);
        }
        [Fact]
        public void Delete_Get_ValidId_ReturnsViewWithModel()
        {
            var album = new Album { Id = 6, Title = "Delete Album", ReleaseYear = 2018 };
            _context.Albums.Add(album);
            _context.SaveChanges();

            var result = _controller.Delete(6) as ViewResult;
            var model = result.Model as AlbumDeleteViewModel;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Delete Album", model.Title);
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
            var album = new Album { Id = 7, Title = "To Be Deleted", ReleaseYear = 2017 };
            _context.Albums.Add(album);
            _context.SaveChanges();

            var result = _controller.DeleteConfirmed(7) as RedirectToActionResult;
            var deletedAlbum = _context.Albums.Find(7);

            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
            Assert.Null(deletedAlbum);
        }
        [Fact]
        public void DeleteConfirmed_InvalidId_ReturnsNotFound()
        {
            var result = _controller.DeleteConfirmed(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}