using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Videos.Models;

public partial class Account
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    public string email { get; set; } = null!;

    [Required]
    [StringLength(60, ErrorMessage = "Must be minimum of 5 in length", MinimumLength = 5)]
    public string password { get; set; } = null!;

    public bool isPublisher { get; set; }

    public bool isAdmin { get; set; }

    public List<Movie>? PublishedMovies { get; set; }
    public List<MovieWatched>? MovieWatcheds { get; set; }
}