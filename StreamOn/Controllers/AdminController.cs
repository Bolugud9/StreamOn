using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Videos.Models;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Videos.Controllers;

[Authorize(Roles = "ADMIN")]
public class AdminController : Controller
{
    ClaimsPrincipal claimUser;
    AppDbContext context;

    public AdminController()
    {
        context = new AppDbContext();
    }

    public IActionResult Index()
    {
        claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {
            var email = claimUser.Claims.ElementAt(0).Value;
            var movies = context.movies.Include(a => a.publisherAccount).ToList();

            var adminMovies = new AdminMovies();

            adminMovies.approvedMovies = movies.FindAll(b => b.approved);
            adminMovies.unApprovedMovies = movies.FindAll(b => b.approved == false);

            return View(adminMovies);
        }
        else
        {
            return RedirectToAction("LogOut", "Home");
        }

        return View();
    }

    public IActionResult VideoInfo(Guid movieId)
    {
        if (!CheckAuthentication())
        {
            return RedirectToAction("LogOut", "Home");
        }

        var movies = context.movies.Where(b => b.MovieId == movieId).ToList();

        return View(movies[0]);
    }

    public async Task< IActionResult> PerformApprove(Guid movieId)
    {
        await Task.Yield();

        if (!CheckAuthentication())
        {
            return RedirectToAction("LogOut", "Home");
        }

        var movie = context.movies.Find(movieId);

        if(movie == null)
        {
            return RedirectToAction("LogOut", "Home");
        }

        if (movie.approved)
        {
            //Unapprove
            movie.approved = false;
            context.SaveChanges();
        }
        else
        {
            //approve
            movie.approved = true;
            context.SaveChanges();
        }

        return RedirectToAction("Index", "Admin");
    }

    private bool CheckAuthentication()
    {
        claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {
            var email = claimUser.Claims.ElementAt(0).Value;
            var movies = context.movies.ToList();
            return true;
        }

        return false;
    }
}

