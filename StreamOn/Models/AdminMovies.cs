using System;
namespace Videos.Models;

public partial class AdminMovies
{
    public List<Movie>? approvedMovies { get; set; }
    public List<Movie>? unApprovedMovies { get; set; }
}

public partial class UserMovies
{
    public List<Movie> watchedMovies { get; set; }
    public List<Movie> newMovies { get; set; }
}