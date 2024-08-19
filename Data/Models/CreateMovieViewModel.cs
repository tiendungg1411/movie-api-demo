using Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class CreateMovieViewModel
    {
        public Guid Id { get; set; }
        [Required (ErrorMessage = "Name of the movie is required")]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Language of the movie is required")]
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }
        public List<Guid> Actors { get; set; }
    }
}
