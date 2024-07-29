using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Videos.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Videos.Controllers;

public class AccountController : Controller
{
    private AppDbContext context;

    public AccountController()
    {
        context = new AppDbContext();
    }

    public IActionResult Login()
    {
        ClaimsPrincipal claimUser = HttpContext.User;

        if (claimUser.Identity.IsAuthenticated)
        {
            if (claimUser.IsInRole("ADMIN"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (claimUser.IsInRole("USER"))
            {
                return RedirectToAction("Index", "User");
            }
            else if (claimUser.IsInRole("PUBLISHER"))
            {
                return RedirectToAction("Index", "Publisher");
            }
            else
            {
                return RedirectToAction("LogOut", "Home");
            }
        }

        return View();
    }

    public IActionResult Registration()
    {
        return View();
    }

    public IActionResult PublisherRegistration()
    {
        return View();
    }

    [HttpPost]
    [ActionName("Login")]
    public async Task<IActionResult> _Login(Account account)
    {
        await Task.Yield();
        ModelState.Clear();

        if (ModelState.IsValid)
        {
            TryValidateModel(account);
            if (ModelState.IsValid)
            {
                List<Claim> claims = new List<Claim>();
                
                var res = await PerformLogin(account);
                switch (res)
                {
                    case SERVER_RES.NONE:
                        ModelState.AddModelError("", "Something Went Wrong");
                        break;
                    case SERVER_RES.WRONG_EMAIL:
                        ModelState.AddModelError("", "Wrong Email");
                        break;
                    case SERVER_RES.WRONG_PASSWORD:
                        ModelState.AddModelError("", "Wrong Password");
                        break;
                    case SERVER_RES.ADMIN:
                        //Redirect to Admin
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "Admin");
                        break;
                    case SERVER_RES.PUBLISHER:
                        //Redirect to Publisher
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "PUBLISHER"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "Publisher");
                        break;
                    case SERVER_RES.USER:
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "USER"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "User");
                        break;
                }
            }
        }
        return View(account);
    }

    [HttpPost]
    [ActionName("Registration")]
    public async Task<IActionResult> _Registration(Account account)
    {
        await Task.Yield();
        ModelState.Clear();

        if (ModelState.IsValid)
        {
            TryValidateModel(account);
            if (ModelState.IsValid)
            {
                List<Claim> claims = new List<Claim>();
                var res = await PerformRegistration(account);
                switch (res)
                {
                    case SERVER_RES.NONE:
                        ModelState.AddModelError("", "Something Went Wrong");
                        break;
                    case SERVER_RES.EMAIL_NOT_AVAILABLE:
                        ModelState.AddModelError("", "Email have been registered. Consider Signing In");
                        break;
                    case SERVER_RES.ADMIN:
                        //Redirect to Admin
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "Admin");
                        break;
                    case SERVER_RES.PUBLISHER:
                        //Redirect to Publisher
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "PUBLISHER"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "Publisher");
                        break;
                    case SERVER_RES.USER:
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "USER"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "User");
                        break;
                }
            }
        }

        return View();
    }

    [HttpPost]
    [ActionName("PublisherRegistration")]
    public async Task<IActionResult> _PublisherRegistration(Account account)
    {
        await Task.Yield();
        ModelState.Clear();

        if (ModelState.IsValid)
        {
            TryValidateModel(account);
            if (ModelState.IsValid)
            {
                List<Claim> claims = new List<Claim>();
                account.isPublisher = true;
                var res = await PerformRegistration(account);
                switch (res)
                {
                    case SERVER_RES.NONE:
                        ModelState.AddModelError("", "Something Went Wrong");
                        break;
                    case SERVER_RES.EMAIL_NOT_AVAILABLE:
                        ModelState.AddModelError("", "Email have been registered. Consider Signing In");
                        break;
                    case SERVER_RES.ADMIN:
                        //Redirect to Admin
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "Admin");
                        break;
                    case SERVER_RES.PUBLISHER:
                        //Redirect to Publisher
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "PUBLISHER"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "Publisher");
                        break;
                    case SERVER_RES.USER:
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, account.email));
                        claims.Add(new Claim(ClaimTypes.Role, "USER"));

                        await PerformCookieAuth(claims);
                        return RedirectToAction("Index", "User");
                        break;
                }
            }
        }

        return View();
    }


    private async Task<SERVER_RES> PerformRegistration(Account account)
    {
        //Check for unique email
        var acc = context.accounts.Where(b => b.email == account.email).ToList();

        if (acc.Count > 0) return SERVER_RES.EMAIL_NOT_AVAILABLE;

        //Assign Id
        account.Id = System.Guid.NewGuid();
        //Hash the password
        account.password = await HashPassword(account.password);

        var updatedVal = context.Entry(account);
        updatedVal.State = EntityState.Added;
        context.SaveChanges();

        if (account.isAdmin) return SERVER_RES.ADMIN;
        if (account.isPublisher) return SERVER_RES.PUBLISHER;
        return SERVER_RES.USER;
    }

    private async Task<SERVER_RES> PerformLogin(Account account)
    {
        //verify from database
        var acc = context.accounts.Where(b => b.email == account.email).ToList();

        if (acc.Count == 0) return SERVER_RES.WRONG_EMAIL;

        var verify_password = await VerifyPassword(account.password, acc[0].password);

        if (!verify_password) return SERVER_RES.WRONG_PASSWORD;

        if (acc[0].isAdmin) return SERVER_RES.ADMIN;
        if (acc[0].isPublisher) return SERVER_RES.PUBLISHER;
        return SERVER_RES.USER;
    }

    private async Task<string> HashPassword(string password)
    {
        await Task.Yield();
        // Generate a random salt
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Create a password hasher using PBKDF2
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

        // Combine salt and hashed password
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashedPassword = new byte[36];
        Array.Copy(salt, 0, hashedPassword, 0, 16);
        Array.Copy(hash, 0, hashedPassword, 16, 20);

        // Convert the combined salt and hashed password to Base64
        string base64HashedPassword = Convert.ToBase64String(hashedPassword);

        return base64HashedPassword;
    }

    private async Task PerformCookieAuth(List<Claim> claims)
    {
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        AuthenticationProperties properties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = true,
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), properties);
    }

    private async Task<bool> VerifyPassword(string inputPassword, string hashedPassword)
    {
        await Task.Yield();
        // Convert the stored hash to bytes
        byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

        // Extract the salt from the stored hash
        byte[] salt = new byte[16];
        Array.Copy(hashedPasswordBytes, 0, salt, 0, 16);

        // Compute the hash of the input password with the extracted salt
        var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(20);

        // Compare the computed hash with the stored hash
        for (int i = 0; i < 20; i++)
        {
            if (hashedPasswordBytes[i + 16] != hash[i])
            {
                return false;
            }
        }

        return true;
    }
}

public enum SERVER_RES
{
    NONE,
    ADMIN,
    PUBLISHER,
    USER,
    WRONG_PASSWORD,
    WRONG_EMAIL,
    EMAIL_NOT_AVAILABLE,
    MOVIE_ADDED,
    MOVIE_REMOVED,
    MOVIE_REMOVE_FAILED
}

