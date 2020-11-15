﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using fanfic.by.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace fanfic.by.Controllers
{
    public class HomeController : Controller
    {
        private readonly FanficContext _context;
        private UserManager<User> _userManager;

        public HomeController(FanficContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var fanficContext = _context.Fanfics.Include(f => f.Genre).Include(g=> g.ImageFanfic);
            foreach ( var fanfic in fanficContext)
            {
                fanfic.ImageFanfic.Link = Request.Scheme + "://" + Request.Host.Value + "/Images/" + fanfic.ImageFanfic.Link;
            }
            return View(await fanficContext.ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
