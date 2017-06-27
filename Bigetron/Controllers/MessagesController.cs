namespace Bigetron.Controllers
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Core.Domain.Users;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using ViewModels;

    public class MessagesController: BaseController
    {
        #region Private Members
        private readonly IConfiguration _configuration;
        private readonly MessageService _messageService;
        private const string captchaVerificationUrl = "https://www.google.com/recaptcha/api/siteverify";
        private static HttpClient client;
        #endregion

        public MessagesController(BTRDbContext dbContext,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration,
            MessageService messageService) : base(dbContext, signInManager, userManager)
        {
            _configuration = configuration;
            _messageService = messageService;
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        ///     POST: messages/query
        /// </summary>
        [HttpPost("Query")]
        public async Task<IActionResult> query([FromBody] CustomerQueryVM cqvm)
        {
            // Check if the captcha is valid
            var captcha = new GoogleCaptchaVM
            {
                secret = _configuration["Authentication:Captcha:Secret"],
                response = cqvm.Captcha
            };
            var response = await client.PostAsync(captchaVerificationUrl,
                new StringContent(JsonConvert.SerializeObject(captcha)));

            if (!response.IsSuccessStatusCode) return new BadRequestResult();
            var emailContent =
                $"Name: {cqvm.Name}\n\nEmail: {cqvm.Email}\n\nTelephone: {cqvm.Telephone}\n\nQuery:\n{cqvm.Query}";
            await _messageService.SendEmailAsync("support@bigetron.gg", "support@bigetron.gg", cqvm.Subject, emailContent, "");
            return Ok();
        }

    }
}
