using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Songify.Data;
using Songify.Entities;
using Songify.Models.AlbumModels;
using System.Security.Claims;

namespace Songify.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext context;
        // Constructor to initialize the database context
        public AlbumsController(ApplicationDbContext context)
        {
            this.context = context;
        }
        // Action to display all albums with optional search functionality
        public IActionResult All(string searchString)
        {
            ViewData["Controller"] = "Albums";
            ViewData["Action"] = "All";
            List<AlbumAllViewModel> albums = context.Albums
                .Select(albumFromDb => new AlbumAllViewModel
                {
                    Id = albumFromDb.Id.ToString(),
                    Title = albumFromDb.Title,
                    ReleaseYear = albumFromDb.ReleaseYear.ToString()
                })
                .ToList();
            if (!string.IsNullOrEmpty(searchString))
            {
                albums = albums.Where(a => a.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return this.View(albums);
        }
        // Action to display the album creation form (only accessible by Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return this.View();
        }
        // POST action to handle the creation of a new album (only accessible by Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(AlbumCreateBindingModel bindingModel)
        {
            if (this.ModelState.IsValid)
            {
                string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Album albumFromDb = new Album
                {
                    Title = bindingModel.Title,
                    ReleaseYear = bindingModel.ReleaseYear
                };

                context.Albums.Add(albumFromDb);
                context.SaveChanges();

                return this.RedirectToAction("All");
            }

            return this.View();
        }
        // Action to display the edit form for an existing album (only accessible by Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var album = context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            var model = new AlbumEditBindingModel
            {
                Id = album.Id,
                Title = album.Title,
                ReleaseYear = album.ReleaseYear
            };

            return View(model);
        }
        // POST action to handle the editing of an album (only accessible by Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(AlbumEditBindingModel model)
        {
            var album = context.Albums.Find(model.Id);
            if (album == null)
            {
                return NotFound();
            }

            album.Title = model.Title;
            album.ReleaseYear = model.ReleaseYear;

            context.Update(album);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        // Action to display the deletion confirmation for an album (only accessible by Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var album = context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            var model = new AlbumDeleteViewModel
            {
                Id = album.Id,
                Title = album.Title,
                ReleaseYear = album.ReleaseYear
            };

            return View(model);
        }
        // POST action to handle the actual deletion of an album (only accessible by Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var album = context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            context.Albums.Remove(album);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        // Action to return the index view (default page for the Albums controller)
        public IActionResult Index()
        {
            return View();
        }
    }
}
