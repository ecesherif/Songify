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
        public LikedSongsController(ApplicationDbContext context, UserManager<SongifyUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
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
        public IActionResult All(string searchString)
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
            if (!String.IsNullOrEmpty(searchString))
            {
                likedSongs = likedSongs.Where(s => s.SongTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return View(likedSongs);
        }
        public ActionResult Add()
        {
            ViewBag.SongId = new SelectList(context.Songs, "Id", "Title");
            return View();
        }

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
