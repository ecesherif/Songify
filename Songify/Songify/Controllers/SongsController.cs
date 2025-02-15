using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Songify.Data;
using Songify.Entities;
using Songify.Models.BandModels;
using Songify.Models.LikedSongsModels;
using Songify.Models.SongModels;
using System.Globalization;
using System.Security.Claims;

namespace Songify.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext context;
        // Constructor to initialize the database context
        public SongsController(ApplicationDbContext context)
        {
            this.context = context;
        } 
        // Action to display all songs with optional search functionality
        public IActionResult All(string searchString)
        {
            ViewData["Controller"] = "Songs";
            ViewData["Action"] = "All";
            var songs = context.Songs.Include(song => song.Album).Include(song => song.Band)
                .Select(song => new SongAllViewModel
                {
                    Id = song.Id.ToString(),
                    Title = song.Title,
                    Duration = song.Duration.ToString(),
                    AlbumName = song.Album.Title,
                    BandName = song.Band.Name
                })
                .ToList();
                if (!string.IsNullOrEmpty(searchString))
                {
                    songs = songs.Where(s => s.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }

            return View(songs);
        }
        // GET action to display the song creation form (only accessible by Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateDropdowns(); 
            return this.View();
        }
        // POST action to handle the creation of a new song (only accessible by Admin)
        [HttpPost]
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
        // GET action to display the edit form for an existing song (only accessible by Admin)
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
        // POST action to handle the editing of an existing song (only accessible by Admin)
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
        // Helper method to populate the dropdown lists for albums and bands
        private void PopulateDropdowns(int? selectedAlbumId = null, int? selectedBandId = null)
        {
            ViewBag.Albums = new SelectList(context.Albums?.ToList() ?? new List<Album>(), "Id", "Title", selectedAlbumId);
            ViewBag.Bands = new SelectList(context.Bands?.ToList() ?? new List<Band>(), "Id", "Name", selectedBandId);
        }
        // GET action to display the song deletion confirmation (only accessible by Admin)
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
        // POST action to handle the actual deletion of a song (only accessible by Admin)
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
        // Default index action that returns the index view (landing page for songs)
        public IActionResult Index()
        {
            return View();
        }
    }
}