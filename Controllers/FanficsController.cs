﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fanfic.by.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace fanfic.by.Controllers
{
    [Authorize]
    public class FanficsController : Controller
    {
        private readonly FanficContext _context;
        private IHostingEnvironment Environment;
        private UserManager<User> _userManager;

        public FanficsController(IHostingEnvironment _environment, FanficContext context, UserManager<User> userManager)
        {
            Environment = _environment;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var fanficContext = _context.Fanfics.Include(f => f.Genre).Include(g => g.ImageFanfic);
            foreach (var fanfic in fanficContext)
            {
                fanfic.ImageFanfic.Link = Request.Scheme + "://" + Request.Host.Value + "/Images/" + fanfic.ImageFanfic.Link;
            }

            var user = await _userManager.GetUserAsync(User);
            var id = _userManager.GetUserId(User);

            ViewBag.UserId = id;

            return View(await fanficContext.ToListAsync());

        }

        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ShortDescription,GenreI")] Fanfic fanfic, IFormFile myFile)
        {
            if (ModelState.IsValid)
            {
                if (myFile != null)
                {
                    // путь к папке Files
                    string path = Environment.WebRootPath +"/Images/" + myFile.FileName;
                    // сохраняем файл в папку Files в каталоге wwwroot
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await myFile.CopyToAsync(fileStream);
                    }
                    ImageFanfic img = new ImageFanfic { Link = myFile.FileName };
                    fanfic.ImageFanfic = img;
                }
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user = await _userManager.GetUserAsync(User);
                var id = _userManager.GetUserId(User);
                fanfic.UserId = id;

                _context.Add(fanfic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Id", fanfic.GenreId);
            return View(fanfic);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fanfic = await _context.Fanfics.FindAsync(id);
            if (fanfic == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", fanfic.GenreId);
            return View(fanfic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ShortDescription,GenreId")] Fanfic fanfic, IFormFile myFile)
        {
            if (id != fanfic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (myFile != null)
                {
                    // путь к папке Files
                    string path = Environment.WebRootPath + "/Images/" + myFile.FileName;
                    // сохраняем файл в папку Files в каталоге wwwroot
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await myFile.CopyToAsync(fileStream);
                    }
                    ImageFanfic img = new ImageFanfic { Link = myFile.FileName };
                    fanfic.ImageFanfic = img;
                }
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user = await _userManager.GetUserAsync(User);
                var idUser = _userManager.GetUserId(User);
                fanfic.UserId = idUser;

                try
                {
                    _context.Update(fanfic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FanficExists(fanfic.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
   
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Id", fanfic.GenreId);
            return View(fanfic);
        }

        // GET: Fanfics/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fanfic = await _context.Fanfics
                .Include(f => f.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fanfic == null)
            {
                return NotFound();
            }

            return View(fanfic);
        }

        // POST: Fanfics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fanfic = await _context.Fanfics.FindAsync(id);
            _context.Fanfics.Remove(fanfic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FanficExists(int id)
        {
            return _context.Fanfics.Any(e => e.Id == id);
        }
    }
}
