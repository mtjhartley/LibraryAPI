using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class BookForCreationDto
    {
        // don't add authorId in this dto
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
