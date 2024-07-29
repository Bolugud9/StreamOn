using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Videos.Models;

public partial class Movie
{
    [Key]
    public Guid MovieId { get; set; }

    [Required]
    public Guid PublisherId { get; set; }
    [Required]
    [StringLength(60, ErrorMessage = "Must be minimum of 3 in length", MinimumLength = 3)]
    public string? MovieName { get; set; }

    [Required]
    [StringLength(600, ErrorMessage = "Must be minimum of 3 in length", MinimumLength = 3)]
    public string? MovieDescription { get; set; }

    [Required(ErrorMessage = "Please upload a video file")]
    [NotMapped]
    [DataType(DataType.Upload)]
    [Display(Name = "Video File")]
    public IFormFile? VideoFile { get; set; }

    [Required(ErrorMessage = "Please upload an image file")]
    [NotMapped]
    [DataType(DataType.Upload)]
    [Display(Name = "Image File")]
    public IFormFile? ImageFile { get; set; }

    public int views { get; set; } = 0;

    public int like { get; set; } = 0;

    public int dislike { get; set; } = 0;

    [Required]
    [DataType(DataType.Date)]
    public DateTime? ReleaseDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime PublishedDate { get; set; }

    [Required]
    public string? MovieLink { get; set; }

    [Required]
    public string? ImageLink { get; set; }


    public float rating { get; set; }

    public bool approved { get; set; }

    public GENRE genre { get; set; }

    public Account? publisherAccount { get; set; }

}

public enum GENRE
{
    Action,
    Adventure,
    Comedy,
    Drama,
    Horror,
    Sci_Fi,
    Fantasy,
    Thriller,
    Mystery,
    Crime,
    Romance,
    Family,
    Animation,
}