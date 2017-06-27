namespace Bigetron.Services.Articles
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Core.Domain.Articles;
    using ECERP.Core;

    /// <summary>
    /// Article Service
    /// </summary>
    public interface IArticleService
    {
        /// <summary>
        /// Gets articles
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="sortOrder">Sort Order</param>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns>Articles</returns>
        IPagedList<Article> GetArticles(
            Expression<Func<Article, bool>> filter = null,
            Func<IQueryable<Article>, IOrderedQueryable<Article>> sortOrder = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets an article by identifier
        /// </summary>
        /// <param name="id">Article Identifier</param>
        /// <returns>Article</returns>
        Article GetArticleById(int id);

        /// <summary>
        /// Inserts an article
        /// </summary>
        /// <param name="article">Article</param>
        void InsertArticle(Article article);
    }
}