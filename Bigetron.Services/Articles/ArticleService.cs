namespace Bigetron.Services.Articles
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Core.Domain.Articles;
    using Data.Abstract;
    using ECERP.Core;

    public class ArticleService : IArticleService
    {
        #region Fields
        private readonly IRepository _repository;
        #endregion

        #region Constructor
        public ArticleService(IRepository repository)
        {
            _repository = repository;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets articles
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="sortOrder">Sort Order</param>
        /// <param name="pageIndex">Page Index</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns>Articles</returns>
        public virtual IPagedList<Article> GetArticles(
            Expression<Func<Article, bool>> filter = null,
            Func<IQueryable<Article>, IOrderedQueryable<Article>> sortOrder = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var skip = pageIndex * pageSize;
            var pagedArticles =
                _repository
                    .Get(filter, sortOrder, skip, pageSize, a => a.Author);
            var totalCount = _repository.GetCount(filter);
            return new PagedList<Article>(pagedArticles, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// Gets an article by identifier
        /// </summary>
        /// <param name="id">Article Identifier</param>
        /// <returns>Article</returns>
        public virtual Article GetArticleById(int id)
        {
            return _repository.GetById<Article>(id, a => a.Author);
        }

        /// <summary>
        /// Gets an article by title
        /// </summary>
        /// <param name="title">Article Title</param>
        /// <returns>Article</returns>
        public virtual Article GetArticleByTitle(string title)
        {
            return _repository.GetOne<Article>(a => a.Title.ToLowerInvariant().Equals(title.ToLowerInvariant()), a => a.Author);
        }

        /// <summary>
        /// Inserts an article
        /// </summary>
        /// <param name="article">Article</param>
        public virtual void InsertArticle(Article article)
        {
            _repository.Create(article);
            _repository.Save();
        }

        /// <summary>
        /// Updates an article
        /// </summary>
        /// <param name="article">Article</param>
        public virtual void UpdateArticle(Article article)
        {
            _repository.Update(article);
            _repository.Save();
        }
        #endregion
    }
}