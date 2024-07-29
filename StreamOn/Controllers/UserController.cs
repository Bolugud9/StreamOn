using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Videos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Videos.Controllers;

[Authorize(Roles = "USER")]
public class UserController : Controller
{
    ClaimsPrincipal claimUser;
    AppDbContext context;

    public UserController()
    {
        context = new AppDbContext();
    }

    public IActionResult Index()
    {
        claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {
            var email = claimUser.Claims.ElementAt(0).Value;
            var acc = context.accounts.Where(b => b.email == email).Include(b => b.MovieWatcheds).ToList();

            if(acc.Count == 0)
            {
                return RedirectToAction("LogOut", "Home");
            }

            var intt  = acc[0]?.MovieWatcheds?.Count;

            var movies = context.movies.Where(b => b.approved == true).ToList();

            var userMovies = new UserMovies();
            userMovies.newMovies = new List<Movie>();
            userMovies.watchedMovies = new List<Movie>();

            for (int i = 0; i < movies.Count; i++)
            {
                if (acc[0].MovieWatcheds?.FirstOrDefault(b => b.MovieId == movies[i].MovieId && b.UserId == acc[0].Id) == null)
                {
                    userMovies.newMovies?.Add(movies[i]);
                }
                else
                {
                    userMovies.watchedMovies?.Add(movies[i]);
                }
            }

            return View(userMovies);
        }
        else
        {
            return RedirectToAction("LogOut", "Home");
        }
        return View();
    }

    public async Task<IActionResult> Watch(Guid movieId)
    {
        claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {
            var movie = context.movies.Where(b => b.MovieId == movieId).ToList();
            var email = claimUser.Claims.ElementAt(0).Value;

            if (movie != null)
            {
                var userId = context.accounts.Where(b => b.email == email).ToList()[0].Id;
                await AddView(movieId);
                await AddWatched(movieId, userId);

                return View(movie[0]);
            }
        }

        return View();
    }

    private async Task AddView(Guid? movieId)
    {
        await Task.Yield();

        if (movieId != null)
        {

            var movie = context.movies.Find(movieId);

            if (movie != null)
            {
                movie.views++;
                context.SaveChanges();
            }
        }
    }

    private async Task AddWatched(Guid movieId, Guid userId)
    {
        await Task.Yield();

        var movie = context.movieWatched.FirstOrDefault(x => x.MovieId == movieId && x.UserId == userId);

        if (movie == null)
        {
            var mW = new MovieWatched();
            mW.MovieId = movieId;
            mW.UserId = userId;

            var updatedVal = context.movieWatched.Entry(mW);
            updatedVal.State = EntityState.Added;
            context.SaveChanges();
        }
    }

    public async Task Like(Guid movieId)
    {
        await Task.Yield();
        claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {
            var email = claimUser.Claims.ElementAt(0).Value;
            var userId = context.accounts.Where(b => b.email == email).ToList()[0].Id;
            var movieWatched = context.movieWatched.FirstOrDefault(x => x.MovieId == movieId && x.UserId == userId);

            if(movieWatched != null)
            {
                var movie = context.movies.Find(movieId);
                if (movieWatched.liked)
                {
                    //Remove like

                    if (movie != null)
                    {
                        movie.like -= 1;
                        movieWatched.liked = false;
                        movie.rating = CalculatePercentage(movie.like, movie.like + movie.dislike);
                    }
                }
                else
                {
                    //Add like

                    if (movie != null)
                    {
                        movie.like += 1;
                        
                        movieWatched.liked = true;

                        movie.rating = CalculatePercentage(movie.like, movie.like + movie.dislike);
                    }

                }
                //var like = context
            }

            context.SaveChanges();
        }
    }

    public async Task Dislike(Guid movieId)
    {
        await Task.Yield();
        claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {
            var email = claimUser.Claims.ElementAt(0).Value;
            var userId = context.accounts.Where(b => b.email == email).ToList()[0].Id;
            var movieWatched = context.movieWatched.FirstOrDefault(x => x.MovieId == movieId && x.UserId == userId);

            if (movieWatched != null)
            {
                var movie = context.movies.Find(movieId);
                if (movieWatched.disliked)
                {
                    //Remove like

                    if (movie != null)
                    {
                        movie.dislike -= 1;
                        movieWatched.disliked = false;
                        movie.rating = CalculatePercentage(movie.like, movie.like + movie.dislike);
                    }
                }
                else
                {
                    //Add like

                    if (movie != null)
                    {
                        movie.dislike += 1;

                        movieWatched.disliked = true;

                        movie.rating = CalculatePercentage(movie.like, movie.like + movie.dislike);
                    }

                }
                //var like = context
            }

            context.SaveChanges();
        }
    }

    private float CalculatePercentage(double like, double whole)
    {
        if (whole == 0)
        {
            // Handle the case where the denominator is zero to avoid division by zero
            //throw new ArgumentException("Whole cannot be zero.");
            return 0;
        }

        float percentage = (float)(like / whole) * 100;
        return percentage;
    }
}

