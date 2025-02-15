using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Songify.Controllers;
using Songify.Data;
using Songify.Entities;
using Songify.Models.BandModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Tests
{
    public class BandsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly BandsController _controller;
        public BandsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SongifyTestDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _controller = new BandsController(_context);
        }
        [Fact]
        public void All_ReturnsViewWithBands()
        {
            _context.Bands.Add(new Band { Id = 1, Name = "Test Band", FormYear = 2000, Country = "USA" });
            _context.SaveChanges();
            var result = _controller.All(null) as ViewResult;
            var model = result.Model as List<BandAllViewModel>;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Test Band", model[0].Name);
        }

        [Fact]
        public void All_FiltersBySearchString()
        {
            _context.Bands.Add(new Band { Id = 2, Name = "Rock Band", FormYear = 2010, Country = "UK" });
            _context.Bands.Add(new Band { Id = 3, Name = "Pop Band", FormYear = 2015, Country = "Canada" });
            _context.SaveChanges();
            var result = _controller.All("Rock") as ViewResult;
            var model = result.Model as List<BandAllViewModel>;
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal("Rock Band", model[0].Name);
        }

        [Fact]
        public void Create_Post_ValidData_RedirectsToAll()
        {
            var bindingModel = new BandCreateBindingModel { Name = "New Band", FormYear = 2024, Country = "Germany" };
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
            var bindingModel = new BandCreateBindingModel { Name = "", FormYear = 0, Country = "" };
            _controller.ModelState.AddModelError("Name", "Required");
            var result = _controller.Create(bindingModel) as ViewResult;
            Assert.NotNull(result);
        }
        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithModel()
        {
            var band = new Band { Id = 4, Name = "Edit Band", FormYear = 2005, Country = "USA" };
            _context.Bands.Add(band);
            _context.SaveChanges();
            var result = _controller.Edit(4) as ViewResult;
            var model = result.Model as BandEditBindingModel;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Edit Band", model.Name);
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
            var band = new Band { Id = 5, Name = "Old Band", FormYear = 2003, Country = "France" };
            _context.Bands.Add(band);
            _context.SaveChanges();
            var model = new BandEditBindingModel { Id = 5, Name = "Updated Band", FormYear = 2024, Country = "Italy" };
            var result = _controller.Edit(model) as RedirectToActionResult;
            var updatedBand = _context.Bands.Find(5);
            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
            Assert.Equal("Updated Band", updatedBand.Name);
        }
        [Fact]
        public void Delete_Get_ValidId_ReturnsViewWithModel()
        {
            var band = new Band { Id = 6, Name = "Delete Band", FormYear = 2018, Country = "Australia" };
            _context.Bands.Add(band);
            _context.SaveChanges();
            var result = _controller.Delete(6) as ViewResult;
            var model = result.Model as BandDeleteViewModel;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Delete Band", model.Name);
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
            var band = new Band { Id = 7, Name = "To Be Deleted", FormYear = 2019, Country = "Japan" };
            _context.Bands.Add(band);
            _context.SaveChanges();
            var result = _controller.DeleteConfirmed(7) as RedirectToActionResult;
            var deletedBand = _context.Bands.Find(7);
            Assert.NotNull(result);
            Assert.Equal("All", result.ActionName);
            Assert.Null(deletedBand);
        }
        [Fact]
        public void DeleteConfirmed_InvalidId_ReturnsNotFound()
        {
            var result = _controller.DeleteConfirmed(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
