using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Models;
using Library.API.Services;
using Library.API.Helpers;

using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Library.API.Entities;
using Microsoft.AspNetCore.Http;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;
        private IUrlHelper _urlHelper;


        public AuthorsController(ILibraryRepository libraryRepository,
            IUrlHelper urlHelper)
        {
            _libraryRepository = libraryRepository;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var nextpageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalpages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextpageLink = nextpageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            // dont need 404 if empty, because it exists, it's just empty.
            return Ok(authors);
        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize
                        });

                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            // requires an additional call
            //if (!_libraryRepository.AuthorExists(id))
            //{
            //    return NotFound();
            //}

            // just check if null, saves a call
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorFromRepo);
            return new JsonResult(author);
        }
        [HttpPost()]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            //when the client messes up
            if (author == null)
            {
                return BadRequest();
            }

            //need to create a new mapping for creation dto to entity
            var authorEntity = Mapper.Map<Author>(author);

            // add to our db context using our database repository service
            _libraryRepository.AddAuthor(authorEntity);

            //call save, if it fails return 500 server error
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
                // return StatusCode(500, "A problem happened with handling your request.");
            }

            // map the entity to a DTO so we can show it
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            // return 201 created response with Method name, anonymous object containing the Id, and the author to return object. 
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(authorFromRepo);
            // cascade when delete is on by default

            if (!_libraryRepository.Save())
            {
                throw new Exception($"An error occured while deleting author with id {id}");
            }

            return NoContent();

        }
        

    }
}
