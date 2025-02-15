using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Songify.Data;
using Songify.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Songify.Models.AlbumModels;
using Songify.Models.LikedSongsModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Songify.Controllers
{
    public class LikedSongsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<SongifyUser> userManager;
        // Constructor to initialize context and userManager
        public LikedSongsController(ApplicationDbContext context, UserManager<SongifyUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        // Action to display all liked songs of the logged-in user
        public ActionResult Index()
        {
            var userId = userManager.GetUserId(User);
            var likedSongs = context.LikedSongs
                .AsNoTracking()
                .Include(l => l.Song)
                .Where(l => l.UserId == userId)
                .ToList();

            return View(likedSongs);
        }
        // Action to display all liked songs in a simplified format
        public IActionResult All(string v)
        {
            var userId = userManager.GetUserId(User);
            List<LikedSongsAllViewModel> likedSongs = context.LikedSongs
                .Where(ls => ls.UserId == userId)
                .Select(lsFromDb => new LikedSongsAllViewModel
                {
                    SongId = lsFromDb.SongId.ToString(),
                    SongTitle = lsFromDb.Song.Title
                })
                .ToList();
            return View(likedSongs);
        }
        // Action to display a form for adding a song to liked songs
        public ActionResult Add()
        {
            ViewBag.SongId = new SelectList(context.Songs, "Id", "Title");
            return View();
        }
        // POST action to handle the form submission for adding a song to liked songs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(LikedSongAddBindingModel model)
        {
            var userId = userManager.GetUserId(User);
            bool alreadyLiked = context.LikedSongs.Any(ls => ls.UserId == userId && ls.SongId == model.SongId);
            if (alreadyLiked)
            {
                ModelState.AddModelError("", "You have already liked this song.");
            }
            else
            {
                var likedSong = new LikedSong
                {
                    UserId = userId,
                    SongId = model.SongId
                };

                context.LikedSongs.Add(likedSong);
                context.SaveChanges();
                return RedirectToAction(nameof(All));
            }
            ViewBag.SongId = new SelectList(context.Songs, "Id", "Title", model.SongId);
            return View(model);
        }
        // Action to display a confirmation view for removing a song from liked songs
        [Authorize]
        public IActionResult Remove(int songId)
        {
            var userId = userManager.GetUserId(User);
            var likedSong = context.LikedSongs.Include(ls => ls.Song).FirstOrDefault(ls => ls.UserId == userId && ls.SongId == songId);
            if (likedSong == null)
            {
                return NotFound();
            }
            var model = new LikedSongRemoveViewModel
            {
                SongId = likedSong.Song.Id,
                SongTitle = likedSong.Song.Title
            };
            return View(model);
        }
        // POST action to handle the actual removal of a liked song
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveConfirmed(int songId)
        {
            var userId = userManager.GetUserId(User);
            var likedSong = context.LikedSongs.FirstOrDefault(ls => ls.UserId == userId && ls.SongId == songId);
            if (likedSong == null)
            {
                return NotFound();
            }

            context.LikedSongs.Remove(likedSong);
            context.SaveChanges();

            return RedirectToAction("All");
        }
    }
}
