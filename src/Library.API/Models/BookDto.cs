﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // do not include teh author to avoid circular reference errors

        // we can however, return the authorId
        public Guid AuthorId { get; set; }
    }
}