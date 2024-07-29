using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Videos.Models;

namespace Videos.Controllers;

[Authorize(Roles = "PUBLISHER")]
public class PublisherController : Controller
{
    AppDbContext context;

    public PublisherController()
    {
        context = new AppDbContext();
    }

    public IActionResult Index()
    {
        ClaimsPrincipal claimUser = HttpContext.User;
        Account _acc = null;

        if (claimUser.Identity.IsAuthenticated)
        {
            var email = claimUser.Claims.ElementAt(0).Value;
            var acc = context.accounts.Where(b => b.email == email).Include(a => a.PublishedMovies).ToList();

            if(acc.Count == 0)
            {
                return RedirectToAction("LogOut", "Home");
            }

            _acc = acc[0];
            var intt = acc[0].PublishedMovies?.Count;

        }

        if (_acc == null)
        {
            return RedirectToAction("LogOut", "Home");
        }

        return View(_acc.PublishedMovies);
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [ActionName("Upload")]
    public async Task<IActionResult> _Upload(Movie movie)
    {
        ClaimsPrincipal claimUser = HttpContext.User;
        if (claimUser.Identity.IsAuthenticated)
        {

            var email = claimUser.Claims.ElementAt(0).Value;
            var acc = context.accounts.Where(b => b.email == email).ToList();

            if (acc.Count > 0)
            {
                // Specify the directory where you want to save the file
                string imageUploadDirectory = Path.Combine(".", "wwwroot",  "uploads", acc[0].Id.ToString(), "Images");
                string videoUploadDirectory = Path.Combine(".", "wwwroot", "uploads", acc[0].Id.ToString(), "Videos");

                // Create the image directory if it doesn't exist
                if (!Directory.Exists(imageUploadDirectory))
                {
                    Directory.CreateDirectory(imageUploadDirectory);
                }

                // Create the video directory if it doesn't exist
                if (!Directory.Exists(videoUploadDirectory))
                {
                    Directory.CreateDirectory(videoUploadDirectory);
                }

                movie.PublisherId = acc[0].Id;
                movie.MovieId = System.Guid.NewGuid();


                movie.ImageLink = await Upload(movie.ImageFile, movie.MovieId, imageUploadDirectory);
                movie.MovieLink = await Upload(movie.VideoFile, movie.MovieId, videoUploadDirectory);

                movie.ImageLink = movie.ImageLink.Remove(2, 8);
                movie.MovieLink = movie.MovieLink.Remove(2, 8);

                switch ( await PerformDatabaseUpload(movie))
                {
                    case SERVER_RES.MOVIE_ADDED:
                        return RedirectToAction("Index", "Publisher");
                        break;
                }

            }

        }

        return View();
    }

    public async Task<IActionResult> Delete(Guid movieId)
    {
        await Task.Yield();
        //Delete in database
        await PerformDelete(movieId);
        return RedirectToAction("Index", "Publisher");
    }

    private async Task<string> Upload(IFormFile file, Guid id, string uploadDirectory)
    {
        // Generate a unique filename to avoid overwriting existing files
        string uniqueFileName = id.ToString() + "_" + file.FileName;

        // Combine the directory and filename to get the full path
        string filePath = Path.Combine(uploadDirectory, uniqueFileName);

        // Save the file to the file system
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
            fileStream.Close();
        }

        return filePath;
    }

    private async Task<SERVER_RES> PerformDatabaseUpload(Movie movie)
    {
        await Task.Yield();
        var updatedVal = context.Entry(movie);
        updatedVal.State = EntityState.Added;
        context.SaveChanges();

        return SERVER_RES.MOVIE_ADDED;
    }

    private async Task<SERVER_RES> PerformDelete(Guid movieId)
    {
        await Task.Yield();
        var movie = context.movies.Find(movieId);

        if(movie != null)
        {
            var updateVal = context.Remove(movie);
            updateVal.State = EntityState.Deleted;
            context.SaveChanges();

            //Delete from filesystem;
            return SERVER_RES.MOVIE_REMOVED;
        }

        return SERVER_RES.MOVIE_REMOVE_FAILED;
    }
}

