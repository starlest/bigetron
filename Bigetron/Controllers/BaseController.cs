namespace Bigetron.Controllers
{
    using System.Security.Claims;
    using Core.Domain.Users;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Route("[controller]")]
    public class BaseController : Controller
    {
        #region Protected Fields
        protected readonly BTRDbContext _dbContext;
        protected readonly SignInManager<User> _signInManager;
        protected readonly UserManager<User> _userManager;
        #endregion

        #region Constructor
        public BaseController(BTRDbContext dbContext,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            // Dependency Injection
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userManager = userManager;
        }
        #endregion

        #region Common Methods
        /// <summary>
        ///     Retrieves the .NET Core Identity User Id
        ///     for the current ClaimsPrincipal.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentUserId()
        {
            // if the user is not authenticated, throw an exception
            return !User.Identity.IsAuthenticated ? null : User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        #endregion

        #region Common Properties
        /// <summary>
        ///     Returns a suitable JsonSerializerSettings object
        ///     that can be used to generate the JsonResult return value
        ///     for this Controller's methods.
        /// </summary>
        protected static JsonSerializerSettings DefaultJsonSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };
        #endregion Common Properties
    }
}