using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Songify.Data;
using Songify.Entities;
using Songify.Models;
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
            List<SongAllViewModel> songs = context.Songs
                .Select(songFromDb => new SongAllViewModel
                {
                    Id = songFromDb.Id.ToString(),
                    Title = songFromDb.Title,
                    Duration = songFromDb.Duration.ToString(),
                    AlbumId = songFromDb.AlbumId.ToString(),
                    BandId = songFromDb.BandId.ToString()
                })
                .ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                songs = songs.Where(s => s.Title.Contains(searchString)).ToList();
            }

            return this.View(songs);
        }

        public IActionResult Create()
        {
            ViewBag.Albums = new SelectList(context.Albums, "AlbumId", "AlbumName");
            ViewBag.Bands = new SelectList(context.Bands, "BandId", "BandName");
            return this.View();
        }

        [HttpPost]

        public IActionResult Create(SongCreateBindingModel bindingModel)
        {
            if (this.ModelState.IsValid)
            {
                string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Song songFromDb = new Song
                {
                    Title = bindingModel.Title,
                    Duration = bindingModel.Duration,
                    AlbumId = bindingModel.AlbumId,
                    BandId = bindingModel.BandId
                };

                context.Songs.Add(songFromDb);
                context.SaveChanges();

                return this.RedirectToAction("All");
            }

            return this.View();
        }
        [Authorize]
        public IActionResult My(string searchString)
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = this.context.Users.SingleOrDefault(u => u.Id == currentUserId);
            if (user == null)
            {
                return null;
            }
            List<LikedSongsListingViewModel> likedSongs = this.context.LikedSongs
                .Where(ls => ls.UserId == user.Id)
                .Select(ls => new LikedSongsListingViewModel
                {
                    Id = ls.Id.ToString(),
                    UserId = ls.UserId,
                    SongId = ls.SongId.ToString(),

                })
                .ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                likedSongs = likedSongs.Where(ls => ls.SongId.Contains(searchString)).ToList();
            }

            return this.View(likedSongs);
        }
        [Authorize]
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

            ViewBag.Albums = new SelectList(context.Albums, "AlbumId", "AlbumName", song.AlbumId);
            ViewBag.Bands = new SelectList(context.Bands, "BandId", "BandName", song.BandId);

            return View(model);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Edit(SongEditBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Albums = new SelectList(context.Albums, "AlbumId", "AlbumName", model.AlbumId);
                ViewBag.Bands = new SelectList(context.Bands, "BandId", "BandName", model.BandId);
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
        public IActionResult Index()
        {
            return View();
        }
    }
}