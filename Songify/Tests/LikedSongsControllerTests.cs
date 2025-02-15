using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Moq;
using Songify.Controllers;
using Songify.Data;
using Songify.Entities;
using Songify.Models.LikedSongsModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;
using Microsoft.EntityFrameworkCore;
namespace Tests
{
    public class LikedSongsControllerTests
    {
        private readonly Mock<UserManager<SongifyUser>> _userManagerMock;
        private readonly ApplicationDbContext _context;
        private readonly LikedSongsController _controller;
        private readonly SongifyUser _testUser;
        public LikedSongsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SongifyTestDb")
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _userManagerMock = new Mock<UserManager<SongifyUser>>(
                Mock.Of<IUserStore<SongifyUser>>(),
                null, null, null, null, null, null, null, null);
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_testUser);
            _testUser = new SongifyUser { Id = "testUserId", UserName = "testuser" };
            _context.Users.Add(_testUser);
            var song1 = new Song { Id = 1, Title = "Song 1" };
            var song2 = new Song { Id = 2, Title = "Song 2" };
            _context.Songs.Add(song1);
            _context.Songs.Add(song2);
            _context.SaveChanges();

            _controller = new LikedSongsController(_context, _userManagerMock.Object);
        }

        private void SetupUserClaims(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            var userPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = userPrincipal }
            };
        }

        [Fact]
        public void Index_ReturnsViewWithLikedSongs()
        {
            SetupUserClaims(_testUser.Id);
            var likedSong = new LikedSong { UserId = _testUser.Id, SongId = 1 };
            _context.LikedSongs.Add(likedSong);
            _context.SaveChanges();
            var result = _controller.Index() as ViewResult;
            Assert.NotNull(result);
            var model = Assert.IsType<List<LikedSong>>(result.Model);
            Assert.Single(model);
            Assert.Equal("Song 1", model[0].Song.Title);
        }

        [Fact]
        public void All_ReturnsFilteredSongs_WhenSearchStringIsProvided()
        {
            SetupUserClaims(_testUser.Id);
            var likedSong1 = new LikedSong { UserId = _testUser.Id, SongId = 1 };
            var likedSong2 = new LikedSong { UserId = _testUser.Id, SongId = 2 };
            _context.LikedSongs.Add(likedSong1);
            _context.LikedSongs.Add(likedSong2);
            _context.SaveChanges();
            var result = _controller.All("Song 1") as ViewResult;
            var model = result.Model as List<LikedSongsAllViewModel>;
            Assert.NotNull(result);
            Assert.Single(model);
            Assert.Equal("Song 1", model[0].SongTitle);
        }

        [Fact]
        public void Add_ReturnsView_WhenModelIsNotValid()
        {
            SetupUserClaims(_testUser.Id);

            var model = new LikedSongAddBindingModel { SongId = 1 };
            _controller.ModelState.AddModelError("SongId", "Song is required.");
            var result = _controller.Add(model) as ViewResult;
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void Add_AddsSongToLikedSongs_WhenValid()
        {
            SetupUserClaims(_testUser.Id);
            var model = new LikedSongAddBindingModel { SongId = 1 };
            var result = _controller.Add(model) as RedirectToActionResult;
            var likedSong = _context.LikedSongs.FirstOrDefault(ls => ls.UserId == _testUser.Id && ls.SongId == 1);
            Assert.NotNull(likedSong);
            Assert.Equal("All", result.ActionName);
        }

        [Fact]
        public void Remove_ReturnsView_WhenLikedSongExists()
        {
            SetupUserClaims(_testUser.Id);
            var likedSong = new LikedSong { UserId = _testUser.Id, SongId = 1 };
            _context.LikedSongs.Add(likedSong);
            _context.SaveChanges();
            var result = _controller.Remove(1) as ViewResult;
            var model = result.Model as LikedSongRemoveViewModel;
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal("Song 1", model.SongTitle);
        }

        [Fact]
        public void Remove_ReturnsNotFound_WhenLikedSongDoesNotExist()
        {
            SetupUserClaims(_testUser.Id);
            var result = _controller.Remove(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void RemoveConfirmed_RemovesSongFromLikedSongs()
        {
            SetupUserClaims(_testUser.Id);
            var likedSong = new LikedSong { UserId = _testUser.Id, SongId = 1 };
            _context.LikedSongs.Add(likedSong);
            _context.SaveChanges();
            var result = _controller.RemoveConfirmed(1) as RedirectToActionResult;
            var removedLikedSong = _context.LikedSongs.FirstOrDefault(ls => ls.UserId == _testUser.Id && ls.SongId == 1);
            Assert.Null(removedLikedSong);
            Assert.Equal("All", result.ActionName);
        }

        [Fact]
        public void RemoveConfirmed_ReturnsNotFound_WhenLikedSongDoesNotExist()
        {
            SetupUserClaims(_testUser.Id);
            var result = _controller.RemoveConfirmed(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
