namespace Bigetron.Core.Domain.Articles
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Users;

    /// <summary>
    /// Represents an article
    /// </summary>
    public class Article: Entity<int>
    {
        /// <summary>
        /// Gets or sets title
        /// </summary>
        [Required, MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets related author identifier
        /// </summary>
        [Required, ForeignKey("Author")]
        public string AuthorId { get; set; }

        /// <summary>
        /// Gets or sets date
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets cover image url
        /// </summary>
        [Required, MaxLength(500)]
        public string CoverImageUrl { get; set; }

        /// <summary>
        /// Gets or sets content
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets related authour
        /// </summary>
        [Required]
        public User Author { get; set; }
    }
}
