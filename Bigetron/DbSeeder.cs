namespace Bigetron
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Domain.Articles;
    using Core.Domain.Users;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using OpenIddict.Core;
    using OpenIddict.Models;

    public class DbSeeder
    {
        #region Private Members
        private readonly BTRDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public DbSeeder(BTRDbContext dbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _applicationManager = applicationManager;
            _configuration = configuration;
        }
        #endregion

        #region Public Methods
        public async Task SeedAsync()
        {
            // Create the Db if it doesn't exist
            _dbContext.Database.EnsureCreated();

            // Create default Application
            if (await _applicationManager.FindByIdAsync("BTR", CancellationToken.None) == null)
                await CreateApplication();

            // Create default Users
            if (await _dbContext.Users.CountAsync() == 0) await CreateUsersAsync();

            if (!_dbContext.Articles.Any()) CreateArticles();
        }
        #endregion

        #region Seed Methods
        private async Task CreateApplication()
        {
            await _applicationManager.CreateAsync(new OpenIddictApplication
            {
                Id = _configuration["Authentication:OpenIddict:ApplicationId"],
                DisplayName = _configuration["Authentication:OpenIddict:DisplayName"],
                RedirectUri = _configuration["Authentication:OpenIddict:RedirectUri"],
                LogoutRedirectUri = _configuration["Authentication:OpenIddict:LogoutRedirectUri"],
                ClientId = _configuration["Authentication:OpenIddict:ClientId"],
                Type = OpenIddictConstants.ClientTypes.Public
            }, CancellationToken.None);
        }

        private async Task CreateUsersAsync()
        {
            // local variables 
            var createdDate = new DateTime(2017, 06, 01, 00, 00, 00);
            const string role_Administrator = "Administrator";

            // Create Roles (if they don't exist yet)
            if (!await _roleManager.RoleExistsAsync(role_Administrator))
                await _roleManager.CreateAsync(new IdentityRole(role_Administrator));

            // Create the "Admin" ApplicationUser account (if it doesn't exist already)
            var user_Admin = new User
            {
                UserName = "Bigetron",
                FirstName = "Bigetron",
                LastName = "Bigetron",
                CreatedDate = createdDate,
                ModifiedDate = createdDate,
                Email = "support@bigetron.gg",
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            // Insert "Admin" into the Database and also assign the "Administrator" role to him.
            if (await _userManager.FindByIdAsync(user_Admin.Id) == null)
            {
                await _userManager.CreateAsync(user_Admin, "Pass4Admin");
                await _userManager.AddToRoleAsync(user_Admin, role_Administrator);
            }

            var user_Nierr = new User
            {
                UserName = "Nierr",
                FirstName = "Aris",
                LastName = "Nugraha",
                CreatedDate = createdDate,
                ModifiedDate = createdDate,
                Email = "aris_nugraha838@yahoo.com",
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            if (await _userManager.FindByIdAsync(user_Nierr.Id) == null)
            {
                await _userManager.CreateAsync(user_Nierr, "Pass4Nierr");
                await _userManager.AddToRoleAsync(user_Nierr, role_Administrator);
            }

            await _dbContext.SaveChangesAsync();
        }

        private void CreateArticles()
        {
            var author_Bigetron = _dbContext.Users.Single(u => u.UserName.Equals("Bigetron"));
            var author_Nierr = _dbContext.Users.Single(u => u.UserName.Equals("Nierr"));
            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Welcome!",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 6, 7),
                    CoverImageUrl = "assets/img/news-default.jpg",
                    Content = "<p>Welcome to Bigetron eSports official website!</p> <p>Stay tuned for more content!"
                },
                new Article
                {
                    Title = "Bintang “ßinx” Pamungkas Bergabung Degan Bigetron!",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 6, 11),
                    CoverImageUrl = "assets/img/binx.jpg",
                    Content = "<div class=\"mb-3\"><img height=\"300\" width=\"300\" src=\"assets/img/binx.jpg\"></div> " +
                              "<p>Bintang Pamungkas yang dikenal sebagai pemain senior di League of Legends Garuda Series (LGS), akan bermain sebagai midlaner Bigetron untuk menggantikan Venus. Bintang Pamungkas yang dikenal sebagai pemain senior di League of Legends Garuda Series (LGS), akan bermain sebagai midlaner Bigetron untuk menggantikan Venus. Pemain Ekko yang handal ini akan menggantikan Venus mulai dari week 3 dikarenakan beberapa halangan yang di alami Venus.</p> " +
                              "<p>Pertandingan LGS Summer 2017 dapat disaksikan melalui livestream di channel youtube League of Legends Indonesia. Info mengenai bisa didapatkan melalui dari <a href=\"lolesports.garena.co.id\" target=\"_blank\">lolesports.garena.co.id</a>.</p>"
                },
                new Article
                {
                    Title = "LoL Guide: Twisted Fate",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 6, 25),
                    CoverImageUrl = "assets/img/twisted-fate.jpg",
                    Content = "<p>Hey Guys! Pada kesempatan kali ini, saya akan membagikan sedikit tips untuk bermain Twisted Fate. Twisted Fate merupakan salah satu champion yang biasanya dimainkan di mid lane dengan menggunakan kartu untuk skill dan auto attacknya. Bermain Twisted Fate sebenarnya gampang-gampang susah. Kenapa? Karena penggunaan skill Pick A Card yang random menyebabkan sejumlah pemain kesulitan kapan untuk mengambil kartu berwarna biru, merah ataupun kuning terutama ketika di gank oleh jungler lawan.</p>" +
                              "<p>Ok, Pertama sebelum bermain kamu perlu mempersiapkan Runes dan Masteries terlebih dahulu.</p><br>" +
                              "<p class=\"font-weight-bold\">Runes</p>" +
                              "<p>Ada beberapa runes yang biasa saya gunakan:</p>" +
                              "<img class=\"img-fluid\" src=\"assets/img/tf-runes.jpg\"><br><br>" +
                              "<p>Quintessence: Movement Speed x3</p>" +
                              "<p>Mark: Magic Penetration x9</p>" +
                              "<p>Seal: Scaling Health x9 (Jika melawan AP champion) atau Armor x9 (Jika melawan AD champion)</p>" +
                              "<p>Glyph: Scaling Cooldown Reduction x6 + Magic Resist x3</p><br>" +
                              "<p class=\"font-weight-bold\">Masteries</p>" +
                              "<img class=\"img-fluid\" src=\"assets/img/tf-masteries.jpg\"><br><br>" +
                              "<p>Kita mengambil Stormraider ketika kalian ingin menjadi mage utility, dan kalian ingin kiting-kiting dari musuh kalian. Namun,jika kalian mengambil damage lebih, ambil Thunderlord. Decree. Ini akan membantu kalian untuk burst satu orang langsung.</p><br>" +
                              "<p class=\"font-weight-bold\">Item Build</p>" +
                              "<ul><li>Starter tentu saja dimulai dengan Doran ring serta 2x potion.</li><li>Lalu, jika memungkinkan untuk membawa pulang uang 1050-1200, kalian dapat meneruskan untuk membeli sheen (more AP + CDR dan kalian tidak kesulitan melawan midlaner musuh) / armseekers guard (resep zonya, ketika melawan ad champion dan kesulitan melawan mereka) / catalyst yang 1100 buat resep Rod of Ages or Hextech GLP-800). Jika kesulitan dan harus pulang lebih awal, belilah dark seal + refillable potion (500gold).</li><li>Item terpenting adalah Lich Bane dan Rod of Ages karena Twisted Fate membutuhkan HP dan MP yang cukup banyak untuk dive or mobile.</li><li>Kapan membeli ROA atau Hextech GLP-800? Jika kalian ingin sustain lebih kalian dapat membeli ROA namun jika kalian ingin bermain menjadi seorang Assassin kalian dapat membeli Hextech GLP-800.</li>\r\n\t\t\t\t\t<li>Sepatu? Boots untuk CDR, 40% CDR = Forever Stun</li><li>Jangan lupa juga kalian untuk membeli Void Staff serta Luden Echo’s untuk movement speed tambahan atau Rabadon Deathcap untuk AP yang lebih banyak lagi.</li></ul><br>" +
                              "<p class=\"font-weight-bold\">Tugas</p>" +
                              "<p>Tugas Twisted Fate sebenarnya cukup mudah, yaitu farm dan jangan mati di lane terutama early game. Ketika sudah mencapai level 6, kalian dapat menggunakan ultimate skill kalian, Destiny untuk bergerak ke top or bot lane dan membantu tim kalian mengambil kill. Setelah itu, kalian dapat kembali ke lane lalu farm lagi. Ketika Cooldown Destiny telah berakhir, kalian dapat melakukan mobilitas lagi ke top or bot lane.</p>" +
                              "<p>Pada mid game sekitar menit 15-25. Tower pertama umumnya akan telah hancur. Twisted Fate dengan Ignite dan Hextech GLP-800, dapat melakukan solo kill kepada jungler, midlaner, support or ad carry musuh. Caranya gampang, kalian perhatikan posisi musuh yang sedang sendirian lalu gunakan Destiny sambil mempersiapkan kartu berwarna kuning. Setelah itu, kalian berpindah ke posisi dekat dengan musuh dan melemparkan kartu kuning kalian serta gunakan Wild Cards dan aktifkan Hextech kalian agar musuh terkena slow dan tidak dapat kabur dari anda. Jika damage yang anda hasilkan masih kurang, anda dapat menggunakan Ignite untuk memastikan kill menjadi milik anda!</p>" +
                              "<p>Di teamfights, tugas kalian adalah memberikan CC kepada frontliner musuh agar memudahkan ad carry kalian untuk menghabisi mereka. CC yang anda berikan tidak harus selalu kartu kuning berupa stun, kalian dapat juga menggunakan kartu berwarna merah untuk memberikan area damage serta slow bagi musuh. Apabila frontliner musuh telah mati, kalian dapat membantu frontliner tim anda dengan menggunakan Destiny atau menculik pemain musuh.</p>" +
                              "<p>Itu sedikit tips tentang cara bermain Twisted Fate. Semoga bermanfaat ya guys!</p>"
                },
                new Article
                {
                    Title = "LoL Guide: Syndra",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 6, 27),
                    CoverImageUrl = "assets/img/syndra.jpg",
                    Content = "<p class=\"font-weight-bold\">Syndra Guide: \"So Much Untapped Power!\"</p>" +
                              "<p>Next champion yang akan saya bahas adalah Syndra. Salah satu champion di mid lane yang sering di pick/ban oleh tim-tim di dunia kompetitif. Kenapa ya? Karena Syndra merupakan Champion dengan kemampuan potensial burst yang sangat besar. Syndra dapat membunuh AD Carry dengan sangat mudah. Seperti biasa, kalian harus mempersiapkan Runes dan Masteries terlebih dahulu.</p><br>" +
                              "<p class=\"font-weight-bold\">Spells</p>" +
                              "<img class=\"img-fluid\" src=\"assets/img/ignite-flash.jpg\"><br><br>" +
                              "<p>Ignite dan Flash for secure kill</p><br>" +
                              "<p class=\"font-weight-bold\">Runes</p>" +
                              "<img class=\"img-fluid\" src=\"assets/img/syndra-runes.jpg\"><br><br>" +
                              "<p>Quintessence: Ability Power x3</p>" +
                              "<p>Mark: Magic Penetration x9</p>" +
                              "<p>Seal: Scaling Health x9</p>" +
                              "<p>Glyph: Scaling Cooldown Reduction x6 + Magic Resist x3</p><br>" +
                              "<p class=\"font-weight-bold\">Masteries</p>" +
                              "<img class=\"img-fluid\" src=\"assets/img/syndra-masteries.jpg\"><br><br>" +
                              "<p>Sama seperti guide sebelumnya, Syndra menggunakan Masteries pada umumnya dengan Thunderlord’s Decree sebagai potensial burstnya.</p><br>" +
                              "<p class=\"font-weight-bold\">Item Build</p>" +
                              "<ul><li>Starter Items: Doran Ring + Potion x2.</li><li>Starter Items: Doran Ring + Potion x2.</li><li>Kemudian, dapat dilanjutkan dengan membeli Lost Chapter untuk sustain Mana di lane.</li><li>Upgrade Lost Chapter kalian menjadi Morellonomicon disertai membeli Boots of Speed.</li><li>Lalu, beli Haunting Guise untuk bonus AP dan Magic Penetration, jangan lupa untuk diupgrade ke Liandry’s Torment dan sepatu menjadi Sorcerers Shoes.</li><li>Next, kalian dapat memilih untuk membeli Void Staff atau Rabadon’s Deathcap tergantung situasi game kalian. Belilah Void Staff jika musuh kalian telah menumpuk sejumlah magic resist atau Rabadon’s Deathcap jika kalian unggul jauh dari musuh dan ingin melakukan aksi solo kill.</li><li>Jangan lupa juga untuk membeli item defensive (seperti, Banshee atau Zhonya) agar skor death kalian tetap 0.</li><li>Beli Elixir untuk last teamfight!. :D</li></ul><br>" +
                              "<p class=\"font-weight-bold\">Tugas</p>" +
                              "<ul><li>Level 1-6: Farm dan poke musuh jika memungkinkan, terlebih jika ada bantuan dari jungler or support. Kalian dapat mendapatkan first blood sangat mudah dengan membawa Ignite :3</li><li>Level 7-11: Be active dan farm. Mobile jika kalian memiliki Unleashed Power tapi bukan berarti kalian melupakan farm kalian.  Jalan menuju Top or Bot lane atau bantu jungler kalian menghancurkan jungler musuh dan kumpulkan stack untuk Bounty Hunter kalian. Dengan bonus 5% dari Bounty Hunter, kalian dapat melelehkan tank musuh atau membunuh AD Carry musuh dengan sangat mudah.</li><li>Level 12-18: Umumnya, permainan telah memasuki menit 20-30. Disini, tugas utama seorang Syndra adalah menghancurkan frontliner musuh atau membunuh AD Carry or Midlaner musuh. Gunakan Unleashed Power dengan bijak!</li></ul><br>" +
                              "<p class=\"font-weight-bold\">Broken Combo Syndra ala Bjergsen:</p>" +
                              "<div class=\"embed-responsive embed-responsive-16by9\"><iframe class=\"embed-responsive-item\" src=\"https://www.youtube.com/embed/YRP_ei3pmi8\"></iframe></div><br><br>" +
                              "Ada banyak combo yang dapat dilakukan dengan Syndra. Agar lebih mudah dipahami, kalian dapat melihat video lama milik Bjergsen, perhatikan bagaimana cara Bjergsen memainkan Combo dari Syndra.<br><br>" +
                              "Bermain Syndra sangatlah menyenangkan, dengan berbagai macam combo serta burst yang sangat tinggi membuat Syndra menjadi Champion yang sering saya gunakan. Nantikan guide dari saya selanjutnya yah!"
                }
            };
            _dbContext.Articles.AddRange(articles);
            _dbContext.SaveChanges();
        }
        #endregion
    }
}