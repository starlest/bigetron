namespace Bigetron.Controllers
{
    using System;
    using System.Linq;
    using AutoMapper;
    using Core.Domain.Articles;
    using Core.Domain.Users;
    using Data;
    using ECERP.Core;
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
        /// GET: ledgeraccounts
        /// </summary>
        /// <returns>An array of all Json-serialized ledger accounts.</returns>
        [HttpGet]
        public IActionResult Get(
            [FromQuery] string sortOrder,
            [FromQuery] int pageIndex,
            [FromQuery] int pageSize)
        {
            pageSize = pageSize == 0 ? int.MaxValue : pageSize;
            var orderBy = GenerateSortOrder(sortOrder);
            var articles = _articleService.GetArticles(null, orderBy, pageIndex, pageSize);
            return
                new JsonResult(
                    Mapper.Map<IPagedList<Article>, PagedListVM<ArticleVM>>(articles),
                    DefaultJsonSettings);
        }

        /// <summary>
        ///     GET: ledgeraccounts/{id}
        /// </summary>
        /// <param name="id">Ledger account identifier</param>
        /// <returns>A Json-serialized object representing a single ledger account.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var article = _articleService.GetArticleById(id);
            if (article == null) return NotFound(new { Error = "Article not found" });
            return new JsonResult(Mapper.Map<Article, ArticleVM>(article), DefaultJsonSettings);
        }
        #endregion

        #region Utilities
        private static Func<IQueryable<Article>, IOrderedQueryable<Article>> GenerateSortOrder(
            string sortOrder)
        {
            switch (sortOrder)
            {
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