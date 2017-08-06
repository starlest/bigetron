namespace Bigetron.Controllers
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoMapper;
    using Core;
    using Core.Domain.Articles;
    using Core.Domain.Users;
    using Data;
    using ECERP.Core;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Services.Articles;
    using ViewModels;

    public class ArticlesController : BaseController
    {
        #region Fields
        private readonly IArticleService _articleService;
        #endregion

        #region Constructor
        public ArticlesController(BTRDbContext dbContext,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IArticleService articleService) : base(dbContext, signInManager, userManager)
        {
            _articleService = articleService;
        }
        #endregion

        #region RESTful Conventions
        /// <summary>
        /// GET: articles
        /// </summary>
        /// <returns>An array of all Json-serialized ledger accounts.</returns>
        [HttpGet]
        public IActionResult Get(
            [FromQuery] string idFilter,
            [FromQuery] string titleFilter,
            [FromQuery] string authorFilter,
            [FromQuery] string dateFilter,
            [FromQuery] string sortOrder,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize)
        {
            pageSize = pageSize == 0 ? int.MaxValue : pageSize;
            var filter = GenerateFilter(idFilter, titleFilter, authorFilter, dateFilter);
            var orderBy = GenerateSortOrder(sortOrder);
            var articles = _articleService.GetArticles(filter, orderBy, pageIndex, pageSize);
            return
                new JsonResult(
                    Mapper.Map<IPagedList<Article>, PagedListVM<ArticleVM>>(articles),
                    DefaultJsonSettings);
        }

        /// <summary>
        ///     GET: articles/{id}
        /// </summary>
        /// <param name="id">Article identifier</param>
        /// <returns>A Json-serialized object representing a single article.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var article = _articleService.GetArticleById(id);
            if (article == null) return NotFound(new { Error = "Article not found" });
            return new JsonResult(Mapper.Map<Article, ArticleVM>(article), DefaultJsonSettings);
        }

        /// <summary>
        /// POST: articles
        /// </summary>
        /// <returns>Creates a new Article and return it accordingly.</returns>
        [HttpPost]
        public IActionResult Add([FromBody] ArticleVM avm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var author_Bigetron = _dbContext.Users.Single(u => u.UserName.Equals("Bigetron"));

                // create a new article with the client-sent json data
                var article = new Article
                {
                    Title = avm.Title,
                    CoverImageUrl = avm.CoverImageUrl,
                    Content = avm.Content,
                    Date = DateTime.Now,
                    AuthorId = author_Bigetron.Id
                };

                _articleService.InsertArticle(article);

                // return the newly-created company to the client.
                return new JsonResult(Mapper.Map<Article, ArticleVM>(article), DefaultJsonSettings);
            }
            catch (Exception)
            {
                // return the error.
                return BadRequest(new { Error = "Check that all the fields are valid." });
            }
        }

        /// <summary>
        /// PUT: articles/{id}
        /// </summary>
        /// <returns>Updates an Article and return it accordingly.</returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ArticleVM avm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (avm == null) return NotFound(new { Error = "Article could not be found." });

                var article = _articleService.GetArticleById(id);

                if (article == null) return NotFound(new { Error = "Article could not be found."});

                // handle the update (on per-property basis)
                article.Title = avm.Title;
                article.CoverImageUrl = avm.CoverImageUrl;
                article.Content = avm.Content;

                _articleService.UpdateArticle(article);

                article = _articleService.GetArticleById(id);

                // return the newly-created company to the client.
                return new JsonResult(Mapper.Map<Article, ArticleVM>(article), DefaultJsonSettings);
            }
            catch (Exception)
            {
                // return the error.
                return BadRequest(new { Error = "Check that all the fields are valid." });
            }
        }
        #endregion

        #region Utilities
        private static Expression<Func<Article, bool>> GenerateFilter(
            string idFilter,
            string titleFilter,
            string authorFilter,
            string dateFilter)
        {
            var filter = PredicateBuilder.True<Article>();

            if (!string.IsNullOrEmpty(idFilter))
                filter = filter.And(
                    x => x.Id.ToString().IndexOf(idFilter, 0, StringComparison.CurrentCultureIgnoreCase) !=
                         -1);

            if (!string.IsNullOrEmpty(titleFilter))
                filter = filter.And(x => x.Title.IndexOf(titleFilter, 0, StringComparison.CurrentCultureIgnoreCase) !=
                                         -1);

            if (!string.IsNullOrEmpty(authorFilter))
                filter = filter.And(
                    x => x.Author.UserName.IndexOf(authorFilter, 0, StringComparison.CurrentCultureIgnoreCase) != -1);

            if (!string.IsNullOrEmpty(dateFilter))
                filter = filter.And(x => x.Date.ToString("dd-MM-yyyy")
                                             .IndexOf(dateFilter, 0, StringComparison.CurrentCultureIgnoreCase) !=
                                         -1);

            return filter;
        }

        private static Func<IQueryable<Article>, IOrderedQueryable<Article>> GenerateSortOrder(
            string sortOrder)
        {
            switch (sortOrder)
            {
                case "id_asc":
                    return x => x.OrderBy(a => a.Id);
                case "id_desc":
                    return x => x.OrderByDescending(a => a.Id);
                case "title_asc":
                    return x => x.OrderBy(a => a.Title);
                case "title_desc":
                    return x => x.OrderByDescending(a => a.Title);
                case "author_asc":
                    return x => x.OrderBy(a => a.Author);
                case "author_desc":
                    return x => x.OrderByDescending(a => a.Author);
                case "date_asc":
                    return x => x.OrderBy(a => a.Date);
                case "date_desc":
                    return x => x.OrderByDescending(a => a.Date);
                default:
                    return null;
            }
        }
        #endregion
    }
}