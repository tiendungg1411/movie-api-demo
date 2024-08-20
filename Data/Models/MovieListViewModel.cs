using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class MovieListViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }
        public List<ActorViewModel> Actors { get; set; }
    }
}
