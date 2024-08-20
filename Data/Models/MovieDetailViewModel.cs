using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class MovieDetailViewModel : MovieListViewModel
    {
        public string Description { get; set; }
    }
}
