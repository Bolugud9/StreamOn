using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Videos.Models;

//[Keyless]
public class MovieWatched
{
    [Key]
    [Required]
    public int id { get; set; }
    [Required]
    public Guid MovieId { get; set; }
    [Required]
    public Guid UserId { get; set; }

    public bool liked { get; set; }

    public bool disliked { get; set; }

    public Account? watcherAccount { get; set; }
}

