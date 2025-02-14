using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Songify.Data;
using Songify.Entities;
using Songify.Models;
using Songify.Models.BandModels;
using Songify.Models.SongModels;
using System.Globalization;
using System.Security.Claims;

namespace Songify.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext context;

        public SongsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult All(string searchString)
        {
            List<SongAllViewModel> songs = context.Songs.Include(songFromDb => songFromDb.Album).Include(songFromDb => songFromDb.Band)
                .Select(songFromDb => new SongAllViewModel
                {
                    Id = songFromDb.Id.ToString(),
                    Title = songFromDb.Title,
                    Duration = songFromDb.Duration.ToString(),
                    AlbumName = songFromDb.Album.Title,
                    BandName = songFromDb.Band.Name                
                })
                .ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                songs = songs.Where(s => s.Title.Contains(searchString)).ToList();
            }

            return this.View(songs);
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateDropdowns(); 
            return this.View();
        }

        [HttpPost]
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(SongCreateBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(bindingModel);
            }

            var songFromDb = new Song
            {
                Title = bindingModel.Title,
                Duration = bindingModel.Duration,
                AlbumId = bindingModel.AlbumId,
                BandId = bindingModel.BandId
            };

            context.Songs.Add(songFromDb);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        [Authorize]
        public IActionResult My(string searchString)
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = this.context.Users.SingleOrDefault(u => u.Id == currentUserId);
            if (user == null)
            {
                return NotFound();
            }
            List<LikedSongsListingViewModel> likedSongs = this.context.LikedSongs
                .Where(ls => ls.UserId == user.Id)
                .Select(ls => new LikedSongsListingViewModel
                {
                    UserId = ls.UserId,
                    SongId = ls.SongId.ToString(),

                })
                .ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                likedSongs = likedSongs.Where(ls => ls.SongId.Contains(searchString)).ToList();
            }

            return View(likedSongs);
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var song = context.Songs.Find(id);
            if (song == null)
            {
                return NotFound();
            }

            var model = new SongEditBindingModel
            {
                Id = song.Id,
                Title = song.Title,
                Duration = song.Duration,
                AlbumId = song.AlbumId,
                BandId = song.BandId
            };

            PopulateDropdowns(song.AlbumId, song.BandId);
            return View(model);
        }
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(SongEditBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model.AlbumId, model.BandId); 
                return View(model);
            }

            var song = context.Songs.Find(model.Id);
            if (song == null)
            {
                return NotFound();
            }

            song.Title = model.Title;
            song.Duration = model.Duration;
            song.AlbumId = model.AlbumId;
            song.BandId = model.BandId;

            context.Update(song);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        private void PopulateDropdowns(int? selectedAlbumId = null, int? selectedBandId = null)
        {
            ViewBag.Albums = new SelectList(context.Albums?.ToList() ?? new List<Album>(), "Id", "Title", selectedAlbumId);
            ViewBag.Bands = new SelectList(context.Bands?.ToList() ?? new List<Band>(), "Id", "Name", selectedBandId);
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var song = context.Songs.Find(id);
            if (song == null)
            {
                return NotFound();
            }

            var model = new SongDeleteViewModel
            {
                Id = song.Id,
                Title = song.Title,
                Duration = song.Duration,
                AlbumId = song.AlbumId,
                BandId = song.BandId
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var song = context.Songs.Find(id);
            if (song == null)
            {
                return NotFound();
            }

            context.Songs.Remove(song);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}