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
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/news-default.jpg",
                    Content = "<p>Welcome to Bigetron eSports official website!</p> <p>Stay tuned for more content!"
                },
                new Article
                {
                    Title = "LoL: Bintang “ßinx” Pamungkas Bergabung Degan Bigetron!",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 6, 11),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/binx.jpg",
                    Content =
                        "<div class=\"mb-3\"><img height=\"300\" width=\"300\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/players/binx.jpg\"></div> " +
                        "<p>Bintang Pamungkas yang dikenal sebagai pemain senior di League of Legends Garuda Series (LGS), akan bermain sebagai midlaner Bigetron untuk menggantikan Venus. Bintang Pamungkas yang dikenal sebagai pemain senior di League of Legends Garuda Series (LGS), akan bermain sebagai midlaner Bigetron untuk menggantikan Venus. Pemain Ekko yang handal ini akan menggantikan Venus mulai dari week 3 dikarenakan beberapa halangan yang di alami Venus.</p> " +
                        "<p>Pertandingan LGS Summer 2017 dapat disaksikan melalui livestream di channel youtube League of Legends Indonesia. Info mengenai bisa didapatkan melalui dari <a href=\"lolesports.garena.co.id\" target=\"_blank\">lolesports.garena.co.id</a>.</p>"
                },
                new Article
                {
                    Title = "LoL Guide: Twisted Fate",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 6, 25),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-twisted-fate-guide/tf.jpg",
                    Content =
                        "<p>Hey Guys! Pada kesempatan kali ini, saya akan membagikan sedikit tips untuk bermain Twisted Fate. Twisted Fate merupakan salah satu champion yang biasanya dimainkan di mid lane dengan menggunakan kartu untuk skill dan auto attacknya. Bermain Twisted Fate sebenarnya gampang-gampang susah. Kenapa? Karena penggunaan skill Pick A Card yang random menyebabkan sejumlah pemain kesulitan kapan untuk mengambil kartu berwarna biru, merah ataupun kuning terutama ketika di gank oleh jungler lawan.</p>" +
                        "<p>Ok, Pertama sebelum bermain kamu perlu mempersiapkan Runes dan Masteries terlebih dahulu.</p><br>" +
                        "<p class=\"font-weight-bold\">Runes</p>" +
                        "<p>Ada beberapa runes yang biasa saya gunakan:</p>" +
                        "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-twisted-fate-guide/tf-runes.jpg\"><br><br>" +
                        "<p>Quintessence: Movement Speed x3</p>" +
                        "<p>Mark: Magic Penetration x9</p>" +
                        "<p>Seal: Scaling Health x9 (Jika melawan AP champion) atau Armor x9 (Jika melawan AD champion)</p>" +
                        "<p>Glyph: Scaling Cooldown Reduction x6 + Magic Resist x3</p><br>" +
                        "<p class=\"font-weight-bold\">Masteries</p>" +
                        "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-twisted-fate-guide/tf-masteries.jpg\"><br><br>" +
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
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-syndra-guide/syndra.jpg",
                    Content = "<p class=\"font-weight-bold\">Syndra Guide: \"So Much Untapped Power!\"</p>" +
                              "<p>Next champion yang akan saya bahas adalah Syndra. Salah satu champion di mid lane yang sering di pick/ban oleh tim-tim di dunia kompetitif. Kenapa ya? Karena Syndra merupakan Champion dengan kemampuan potensial burst yang sangat besar. Syndra dapat membunuh AD Carry dengan sangat mudah. Seperti biasa, kalian harus mempersiapkan Runes dan Masteries terlebih dahulu.</p><br>" +
                              "<p class=\"font-weight-bold\">Spells</p>" +
                              "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-syndra-guide/ignite-flash.jpg\"><br><br>" +
                              "<p>Ignite dan Flash for secure kill</p><br>" +
                              "<p class=\"font-weight-bold\">Runes</p>" +
                              "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-syndra-guide/syndra-runes.jpg\"><br><br>" +
                              "<p>Quintessence: Ability Power x3</p>" +
                              "<p>Mark: Magic Penetration x9</p>" +
                              "<p>Seal: Scaling Health x9</p>" +
                              "<p>Glyph: Scaling Cooldown Reduction x6 + Magic Resist x3</p><br>" +
                              "<p class=\"font-weight-bold\">Masteries</p>" +
                              "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-syndra-guide/syndra-masteries.jpg\"><br><br>" +
                              "<p>Sama seperti guide sebelumnya, Syndra menggunakan Masteries pada umumnya dengan Thunderlord’s Decree sebagai potensial burstnya.</p><br>" +
                              "<p class=\"font-weight-bold\">Item Build</p>" +
                              "<ul><li>Starter Items: Doran Ring + Potion x2.</li><li>Starter Items: Doran Ring + Potion x2.</li><li>Kemudian, dapat dilanjutkan dengan membeli Lost Chapter untuk sustain Mana di lane.</li><li>Upgrade Lost Chapter kalian menjadi Morellonomicon disertai membeli Boots of Speed.</li><li>Lalu, beli Haunting Guise untuk bonus AP dan Magic Penetration, jangan lupa untuk diupgrade ke Liandry’s Torment dan sepatu menjadi Sorcerers Shoes.</li><li>Next, kalian dapat memilih untuk membeli Void Staff atau Rabadon’s Deathcap tergantung situasi game kalian. Belilah Void Staff jika musuh kalian telah menumpuk sejumlah magic resist atau Rabadon’s Deathcap jika kalian unggul jauh dari musuh dan ingin melakukan aksi solo kill.</li><li>Jangan lupa juga untuk membeli item defensive (seperti, Banshee atau Zhonya) agar skor death kalian tetap 0.</li><li>Beli Elixir untuk last teamfight!. :D</li></ul><br>" +
                              "<p class=\"font-weight-bold\">Tugas</p>" +
                              "<ul><li>Level 1-6: Farm dan poke musuh jika memungkinkan, terlebih jika ada bantuan dari jungler or support. Kalian dapat mendapatkan first blood sangat mudah dengan membawa Ignite :3</li><li>Level 7-11: Be active dan farm. Mobile jika kalian memiliki Unleashed Power tapi bukan berarti kalian melupakan farm kalian. Jalan menuju Top or Bot lane atau bantu jungler kalian menghancurkan jungler musuh dan kumpulkan stack untuk Bounty Hunter kalian. Dengan bonus 5% dari Bounty Hunter, kalian dapat melelehkan tank musuh atau membunuh AD Carry musuh dengan sangat mudah.</li><li>Level 12-18: Umumnya, permainan telah memasuki menit 20-30. Disini, tugas utama seorang Syndra adalah menghancurkan frontliner musuh atau membunuh AD Carry or Midlaner musuh. Gunakan Unleashed Power dengan bijak!</li></ul><br>" +
                              "<p class=\"font-weight-bold\">Broken Combo Syndra ala Bjergsen:</p>" +
                              "<div class=\"embed-responsive embed-responsive-16by9\"><iframe class=\"embed-responsive-item\" src=\"https://www.youtube.com/embed/YRP_ei3pmi8\"></iframe></div><br><br>" +
                              "Ada banyak combo yang dapat dilakukan dengan Syndra. Agar lebih mudah dipahami, kalian dapat melihat video lama milik Bjergsen, perhatikan bagaimana cara Bjergsen memainkan Combo dari Syndra.<br><br>" +
                              "Bermain Syndra sangatlah menyenangkan, dengan berbagai macam combo serta burst yang sangat tinggi membuat Syndra menjadi Champion yang sering saya gunakan. Nantikan guide dari saya selanjutnya yah!"
                },
                new Article
                {
                    Title = "Bigetron TV: Welcome Jennifer \"Panini\" Pingkan!",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 6, 28),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/pingkan.jpg",
                    Content =
                        "<p>Hellooo guys! Bigetron akan kehadiran seorang streamer wanita nih, dia bernama <a href=\"https://www.facebook.com/Jennifer-Pannini-Pingkan-273094776498937/\" target=\"_blank\">Jennifer \"Pannini\" Pingkan</a>. Nantinya Jennifer akan melakukan streaming beberapa games bersama Bigetron.</p>" +
                        "<p>Jennifer akan melakukan streaming beberapa games, salah satunya adalah League of Legends. Penulis juga melakukan sedikit wawancara, berikut cuplikan wawancara bersama Jennifer.</p><br><br>" +
                        "<p>(P: Penulis; J: Jennifer)</p><hr>" +
                        "<p>P: Selamat siang mba Jennifer. Saya Aris penulis dari Bigetron.</p>" +
                        "<p>J: Hallooo, iya salam kenal.</p><br>" +
                        "<p>P: Mba Jennifer nanti berencana jadi streamer yah?</p>" +
                        "<p>J: Iya.</p><br>" +
                        "<p>P: Sudah pernah jadi streamer sebelumnya?</p>" +
                        "<p>J: Belum, baru kali ini hehe.</p><br>" +
                        "<p>P: Apa yang membuat mba Jennifer tertarik menjadi seorang streamer?</p>" +
                        "<p>J: Tertarik mencoba hal yang baru aja. Selain itu, sekarang sudah banyak streamer saat ini. Jadinya, ingin mencoba berkompetitif dengan yang lain juga.</p><br>" +
                        "<p>P: Nanti streaming games apa aja nih ceritanya?</p>" +
                        "<p>J: Untuk saat ini, mau streaming LOL, dota2, CSGO dulu. Tapi nanti masih bisa nambah lagi cuma belum kepikiran aja hehe.</p><br>" +
                        "<p>P: Jadwalnya sendiri gimana? Apa sudah ada?</p><p>J: Jadwal streamingnya nanti hari Selasa, Kamis, Jumat jam 6 - 9 malem.</p><hr><br>" +
                        "<p>Itulah wawancara singkat bersama Jennifer Pingkan. Jangan lewatkan streaming langsung bersama Jennifer Pingkan yah di channel youtube <a href=\"https://www.youtube.com/channel/UCIJ01RpgMcFu6vrViEy2irQ\" target=\"_blank\">Bigetron TV</a> yang akan dimulai pada tanggal 30 Juni 2017 jam 6! Don\'t miss it ya guys!</p>"
                },
                new Article
                {
                    Title = "Bigetron TV: Welcome Rangga \"Xer1zo\" Hanan!",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 6, 29),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/rangga.jpeg",
                    Content =
                        "<p>Sore guys! Seperti yang sudah kalian baca kemarin, Jennifer \"Pannini\" Pingkan akan menjadi streamer tanggal 30 nanti. Nah, selain Jennifer, Bigetron eSports juga kedatangan salah satu streamer baru nih guys. Dia akan fokus streaming pada game Counter Strike: Global Offensive (CS:GO). Siapakah dia?</p>" +
                        "<p>Dia bernama <a href=\"https://www.facebook.com/xer1zo/\" target=\"_blank\">Rangga \"Xer1zo\" Hanan</a>. Sudah pada kenal belum sama Rangga? Kalau belum, ini sedikit informasi tentang Rangga. Rangga Hanan ini salah satu pemain Counter Strike Online (CSO) dan CS:GO. Rangga sempat melakukan streaming untuk tim ExOne Gaming namun ia melakukanya secara personal.</p>" +
                        "<p>Yuk, kita lihat hasil wawancara penulis dengan Rangga Hanan.</p><br><br>" +
                        "<p>(P: Penulis; R: Rangga)</p><hr>" +
                        "<p>P: Selamat siang mas Rangga. Saya Aris penulis dari Bigetron eSports. Boleh meminta waktunya sebentar?</p>" +
                        "<p>R: Hey, mas Aris. Boleh mas, silahkan.</p><br>" +
                        "<p>P: Mas Rangga, apa dulu sudah pernah menjadi seorang streamer sebelumnya?</p>" +
                        "<p>R: Dulu sih pernah. Sebelumnya jadi streamer CSO dan CSGO untuk tim ExOne. Tapi bukan jadi profesional streamer tapi masih sebatas jadi personal streamer saja.</p><br>" +
                        "<p>P: Ohh. Apa yang menjadi motivasi mas Rangga untuk menjadi seorang streamer?</p>" +
                        "<p>R: Ingin mengenalkan dan mendekatkan diri dengan fans di game mas.</p><br>" +
                        "<p>P: Wah, keren mas Rangga.. Nanti streaming game apa aja mas rencananya?</p>" +
                        "<p>R: Dulu sih CSO dan CSGO saja. Di Bigetron nanti, rencananya mau fokus ke CSGO saja dulu.</p><br>" +
                        "<p>P: Apakah sudah ada jadwal streamingnya mas?</p>" +
                        "<p>R: Sudah ada, enaknya sih nanti di hari senin dan selasa itu jam 6 sampai jam 9 malem. Terus, nanti juga ada di hari sabtu mas di jam 2 siang sampai 5 sore.</p><br>" +
                        "<p>P: Wih.. mantab. Mas Rangga tadi ada cerita ingin lebih mendekatkan diri kepada fans. Ada pesan yang ingin disampaikan kepada para fans?</p>" +
                        "<p>R: Hmm.. Pesannya sih saya ingin memajukan Bigetron sebagai salah satu organisasi eSports terbaik di Indonesia serta kedepannya eSports di Indonesia dapat menjadi lahan pekerjaan utama seperti negara-negara luar bukan lagi sebagai hobi semata. Jadi, jangan lupa tonton saya ya guys!</p><br>" +
                        "<p>P: Hahaha. mantab kali pesannya. Baiklah, terima kasih mas Rangga untuk waktunya.</p>" +
                        "<p>R:  Siap mas Aris, terima kasih juga.</p><hr><br>" +
                        "<p>Ok guys. Itulah wawancara singkat penulis bersama Rangga Hanan. Bagi pecinta CSGO, sangat dianjurkan untuk melihat aksi Rangga bermain nantinya. Rangga akan bermain pada tanggal 03/07/2017. Jangan lupa untuk menyaksikan Rangga Hanan di <a href=\"https://www.youtube.com/channel/UCIJ01RpgMcFu6vrViEy2irQ\" target=\"_blank\">Bigetron TV</a> yah guys!</p><br>"
                },
                new Article
                {
                    Title = "Thresh Lantern +200 IQ vs -200 IQ!",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 6, 29),
                    CoverImageUrl =
                        "https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-thresh-lantern-%2B200iq-vs--200iq/thresh-lantern.jpg",
                    Content =
                        "<p>Thresh merupakan salah satu champion support yang sedang populer baik di kancah kompetitif ataupun di solo queue. Salah satu keunikan yang membuat Thresh sangat disukai banyak orang adalah bagaimana caranya mendaratkan hook ke pemain serta penggunaan lanternnya sendiri. Lantern dari Thresh dapat memberi keselamatan namun di sisi lain, lanternnya dapat menjadi sebuah bencana bagi suatu tim.</p>" +
                        "<p>Tapi, kita tidak dapat menilai itu salah Thresh. Niatnya sungguh baik tapi terkadang pemain lain juga berperan akan tindakan hal ini. Betul kan? Mau lihat momen-momen terbaik dan terlucu bersama Thresh?</p>" +
                        "<p>Langsung cekidot video dibawah aja ya guys!</p>" +
                        "<div class=\"embed-responsive embed-responsive-16by9\"><iframe class=\"embed-responsive-item\" src=\"https://www.youtube.com/embed/G9WKsiAwRT0\"></iframe></div><br>" +
                        "<p>Credits: <a href=\"https://www.youtube.com/channel/UC2_q_FIJrbFad8tSUV1NIwg\" target=\"_blank\">Challenger</a></p><br>" +
                        "<p>Hoho.. Menarik yah videonya guys! Bagaimana andil dari pemain juga ikut berperan terhadap tim mereka dan bukan hanya kesalahan dari support terus. Meskipun yah, ada juga Thresh yang bermain dengan -200 IQ. Haha. Hal yang terpenting adalah jangan lupa untuk menikmati setiap permainan kalian guys, baik menang atau kalah. Have fun guys!</p>"
                },
                new Article
                {
                    Title = "LoL Guide: Corki Mid",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 7, 1),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-corki-mid-guide/corki.jpg",
                    Content =
                        "<p>Salah satu meta yang sedang populer saat ini, yaitu Corki mid. Dengan kemampuan mixed damage (physical and magic damage), Corki menjadi pilihan utama di mid laner saat ini.</p>" +
                        "<p>Corki akan mencapai power spikenya ketika ia telah membeli Trinity Force karena kemampuan Corki dan passive skill dari Trinity Force yang sangat cocok dengan kebutuhan Corki.</p>" +
                        "<p>Yuk kita lihat apa saja runes dan masteries untuk Corki:</p><br>" +
                        "<p class=\"font-weight-bold\">Spells: Heal dan Flash (bisa juga diganti dengan Cleanse sesuai kebutuhan saja)</p><br>" +
                        "<p class=\"font-weight-bold\">Runes</p>" +
                        "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-corki-mid-guide/corki-mid-runes.jpg\"><br><br>" +
                        "<p class=\"font-weight-bold\">Masteries</p>" +
                        "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-corki-mid-guide/corki-mid-masteries.jpg\"><br><br>" +
                        "<p class=\"font-weight-bold\">Item Sets</p>" +
                        "<ol><li>Starter items: Doran Shield + 1 potion atau Corrupting potion</li><li>First item yang harus kalian beli adalah Trinity Force atau Hexdrinker apabila kalian melawan AP Mid Burst seperti Le Blanc.</li><li>Upgrade sepatu kalian menjadi Sorcerer Shoes. Kemudian lanjutkan dengan 4 items berikutnya</li><li>Di sisa 4 slot selanjutnya, kalian dapat membeli Statikk Shiv, Infinity Edge, Rapid Fire Cannon, dan Mercury Scimitar.</li><li>Kalian dapat mengganti Mercury Scimitar dengan Maw of Marmoritus sesuai kondisi dan kebutuhan kalian.</li></ol><br>" +
                        "<p>Contoh dari full items Corki:</p>" +
                        "<img class=\"img-fluid\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/lol-corki-mid-guide/corki-mid-items.jpg\"><br><br>" +
                        "<p class=\"font-weight-bold\">Tugas</p>" +
                        "<ol><li>Early Game: Farm dan bermain safe. Bermain Corki itu harus sabar sama seperti AD Carry lainnya yang membutuhkan beberapa items untuk mencapai power spike. Corki membutuhkan Trinity Force untuk memenuhi kebutuhannya. Jika paket kalian telah ada, kalian dapat melakukan mobile ke lane lain, seperti top or bot.</li><li>Mid Game: Sekitar 20-30 menit, kalian harusnya telah membeli 2-3 items. Pada teamfights kecil, kalian dapat membantu teman kalian menghancurkan frontliner musuh. Dengan mixed damage dari Corki, tanker musuh seharusnya tidak akan kuat menahan damage dari Corki.</li><li>Late Game: Layaknya bermain sebagai AD Carry, kalian harus berada di belakang dan memberikan damage output sebanyak mungkin. Jika package telah tersedia, kalian harus menggunakan dengan bijak, karena satu kesalahan dapat berakibat fatal bagi tim.</li></ol><br>" +
                        "<p>Ini saya berikan salah satu contoh yang tidak tepat dari Senior Sneaky. Jangan ditiru ya guys!</p><br>" +
                        "<div class=\"embed-responsive embed-responsive-16by9\"><iframe class=\"embed-responsive-item\" src=\"https://www.youtube.com/embed/5K1j35xVcSU\"></iframe></div><br><br>" +
                        "<p>Ok guys, Itulah guide Corki dari saya. Sebagai bonus, saya tambahkan cara bermain Corki dari Senpai Apdo (Challenger Korea No. 1). Stay tuned terus di Bigetron eSports untuk guide selanjutnya.</p><br>" +
                        "<div class=\"embed-responsive embed-responsive-16by9\"><iframe class=\"embed-responsive-item\" src=\"https://www.youtube.com/embed/zOkROiuPn10\"></iframe></div>"
                },
                new Article
                {
                    Title = "Mobile Arena: RoboX Menjadi Kapten",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 6, 0, 0, 0),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/robox-announcement.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/players/robox-announcement.jpg\"><br><br>" +
                        "<p>Bigetron eSports mulai bergerak cepat dengan membentuk tim baru bagi Mobile Arena (Morena). Sebagai salah satu game yang baru saja dirilis, Mobile Arena telah menyedot perhatian beberapa player Indonesia, salah satunya pro player League of Legends (LoL) Indonesia. Dia bernama pin pin atau dikenal dengan nama RoboX.</p>" +
                        "<p>Bersama Bigetron, Pin pin akan menjadi kapten tim Bigetron pada Mobile Arena. Apa saja yang akan dilakukan pin pin bersama Bigetron? Mari kita simak wawancara dengan kapten tim Bigetron satu ini.</p><br><br>" +
                        "<p>P: Penulis; R: RoboX</p><hr>" +
                        "<p>P: Siang mas Pin pin, ini saya Aris dari Bigetron Media.</p>" +
                        "<p>R: Iya, halo Ris. Aris mana nih? Wkwkwk.</p><br>" +
                        "<p>P: Aris dari LoL pin.</p>" +
                        "<p>R: Wew.</p><br>" +
                        "<p>P: Boleh nanya-nanya sedikit pin?</p>" +
                        "<p>R: Ok, boleh.</p><br>" +
                        "<p>P: Anda terkenal di dunia LoL bersama tim Terserah sebelumnya. Apa yang membuat anda berpindah dari LoL ke Mobile Arena?</p>" +
                        "<p>R: Karena bentrok dengan kerjaan jadinya ga bisa komit full time di LoL... sedangkan kalo di Morena gw bisa sempet2in main setiap ada waktu luang.</p><br>" +
                        "<p>P: Oh, sayang sekali ya. Sudah berapa lama anda bermain Mobile Arena?</p>" +
                        "<p>R: Sejak pertama kali di launching.</p><br>" +
                        "<p>P: Apa kelebihan Mobile Arena dibanding game sejenis?</p>" +
                        "<p>R: Karena grafiknya lebih keren dan gameplaynya seru.</p><br>" +
                        "<p>P: Apa target yang ingin anda capai bersama Bigetron?</p>" +
                        "<p>R: Ingin jadi juara 1 se-Indonesia untuk sekarang .. Kalo sudah tercapai ya International.</p><br>" +
                        "<p>P: Wihh, mantab. Apakah ada turnamen yang akan diikuti dalam waktu dekat?</p>" +
                        "<p>R: Ada, yang diadakan oleh duniaku.net, di Balai Kartini nanti.</p><br>" +
                        "<p>P: Ada pesan-pesan yang ingin disampaikan kepada para penggemar?</p>" +
                        "<p>R: Tetap berusaha dan jangan pernah menyerah buat dapetin apa yang kalian impikan.</p><br>" +
                        "<p>P: Keren pin. Haha. Terima kasih ya Pin atas waktunya.</p>" +
                        "<p>R: Iya, sama-sama Ris.</p><hr><br>" +
                        "<p>Itulah wawancara bersama kapten tim Bigetron, RoboX. Siapakah keempat pemain yang akan berjuang bersama RoboX di Balai Kartini nanti? Jangan kemana-mana guys, stay tuned terus di Bigetron.gg ya!!</p>"
                },
                new Article
                {
                    Title = "Mobile Arena: Rank 1 Steven \"Age\" Bergabung Dengan Bigetron eSports",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 6, 0, 0, 1),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/age-announcement.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/players/age-announcement.jpg\"><br><br>" +
                        "<p>Pemain kedua yang bergabung bersama Bigetron eSports adalah Steven Age. Steven Age merupakan salah satu pemain top tier yang pernah mencapai peringkat pertama pada tier Grand Conqueror di Mobile Arena. Steven akan menggunakan nama \"BTR · Age\" untuk bermain Mobile Arena. Steven bersama penulis akan menceritakan tentang visi dan misinya bersama Bigetron.</p><br><br>" +
                        "<p>P: Penulis; S: Steven Age</p><hr>" +
                        "<p>P: Menurut informasi yang saya terima, kamu pernah mencapai peringkat pertama di Mobile Arena. Apakah benar?</p>" +
                        "<p>S: Iya, tapi sekarang lagi di posisi kedua. Yah namanya game, ada kalah ada juga menang. Tidak selamanya berada diatas.</p><br>" +
                        "<p>P: Apa yang membuatmu tertarik bermain Mobile Arena?</p>" +
                        "<p>S: Karena Mobile Arena gamenya lebih simpel dibanding game-game lain dan tidak seperti game di komputer lalu saya bisa bermain dimana saja. Misalnya, ketika saya sedang ngumpul sama teman-teman, jadi saya masih bisa bersosialisasi juga sama yang lain. Kalau di komputer, saya harus stay di depan komputer terus.</p><br>" +
                        "<p>P: Apa yang membuat kamu memilih Bigetron?</p>" +
                        "<p>S: Karena, Bigetron itu memiliki singkatan \"Big\"etron yang berarti besar kan. Yah, jadinya ingin membuat Bigetron menjadi sebuah organisasi gamers yang bersar dan membuat Bigetron menjadi tim yang kuat dan hebat.</p><br>" +
                        "<p>P: Apa target yang ingin kamu capai dalam waktu dekat ini?</p>" +
                        "<p>S: Nanti tanggal 29 Juli saya dan teman-teman akan bertanding di Balai Kartini. Jadinya, target saya adalah menjadi juara dan membuat bangga teman-teman saya serta bagi Bigetron juga.</p><br>" +
                        "<p>P: Siapa teman yang paling dekat dengan kamu di Bigetron?</p>" +
                        "<p>S: Untuk saat ini, belum ada yang benar-benar dekat. Kita masih sebatas teman bermain Mobile Arena. Tapi, Saya yakin kita pasti bakal lebih dekat lagi karena kita memiliki hobi yang sama. Kita bermain dan berlatih Mobile Arena setiap hari jadinya kita pasti akan lebih dekat lagi.</p><br>" +
                        "<p>P: Ada kata-kata yang ingin disampaikan kepada para pembaca Bigetron?</p>" +
                        "<p>S: Hmm.. Pesan saya cukup sederhana, yaitu semoga gamers di Indonesia dapat menjadi lebih hebat dan go International. Karena jika tidak dimulai dari sekarang siapa lagi yang akan mulai.</p><hr><br>" +
                        "<p>Itulah wawancara bersama Age. Siapakah ketiga pemain yang akan berjuang bersama RoboX dengan Age? Stay tuned!</p>"
                },
                new Article
                {
                    Title = "Mobile Arena: Wahyu \"SiMontok\" Febri Bergabung Dengan Bigetron eSports",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 6, 0, 0, 2),
                    CoverImageUrl =
                        "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/simontok-announcement.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/players/simontok-announcement.jpg\"><br><br>" +
                        "<p>Selanjutnya, Bigetron akan memperkenalkan Wahyu \"BTR SiMontok\" Febri sebagai anggota ketiga di Mobile Arena. Saat ini, SiMontok tengah berada di posisi ketujuh pada tier Grand Conqueror.\r\nBersama Bigetron, SiMontok ingin meraih prestasi terbaik di Mobile Arena. Penulis mengajak SiMontok untuk bercerita lebih lanjut tentang harapan dan keinginan kedepannya.</p><br><br>" +
                        "<p>P:Penulis; W: Wahyu \"SiMontok\" Febri</p><hr>" +
                        "<p>P: Kamu merupakan orang ketiga yang bergabung bersama Bigetron untuk bermain Mobile Arena. Apa yang membuat kamu memilih tim ini?</p>" +
                        "<p>W: Saya tertarik untuk memajukan organisasi Bigetron menjadi lebih besar dan lebih baik lagi.</p><br>" +
                        "<p>P: Sudah berapa lama kamu bermain Mobile Arena?</p>" +
                        "<p>W: Sejak dirilis saya sudah bermain Mobile Arena.</p><br>" +
                        "<p>P: Apa target yang ingin kamu capai?</p>" +
                        "<p>W: Saya ingin meraih prestasi, juara satu untuk tingkat nasional.</p><br>" +
                        "<p>P: Di Bigetron ini, kamu paling dekat sama siapa?</p>" +
                        "<p>W: Saya dekat dengan semuanya. Saya biasa latihan bersama semuanya tapi kalau lagi duo, saya biasanya bersama Age dan VieL, Age itu partner duet terbaik saya sejak pertama kali bermain.</p><br>" +
                        "<p>P: Apa saja yang kamu lakukan jika sedang diluar waktu latihan bersama?</p>" +
                        "<p>W: Saya biasanya bermain untuk mengajarkan para pemain baru. Jadi, terkadang saya pakai akun smurf buat bantu atau ajarin temen-temen yang tertarik buat main.</p><br>" +
                        "<p>P: Kamu tadi mengatakan kalau kamu bermain bersama Age dan VieL di waktu senggang. Menurut kamu VieL itu orangnya bagaimana?</p>" +
                        "<p>W: VieL itu orang yang baik dan ramah, kalau didalam game, dia itu tank yang khusus cover saya. Haha.</p><br>" +
                        "<p>P: Nick kamu kan \"SiMontok\" didalam game. Kenapa memilih nama seperti itu?</p>" +
                        "<p>W: Karena montok itu menarik aja dan lucu juga terus bisa menarik perhatian orang.</p><br>" +
                        "<p>P: Ada kata-kata yang ingin disampaikan kepada para pembaca Bigetron?</p>" +
                        "<p>W: Well, pesan saya. Jangan mudah menyerah belajar, play fun tapi no emosi dan tetap rendah hati.</p><hr><br>" +
                        "<p>Ok guys. Itulah diskusi singkat bersama SiMontok yang menjadi pemain ketiga bergabung bersama Bigetron. Tunggu info kami selanjutnya yah untuk pemain keempat dan pemain kelima. See ya guys!</p>"
                },
                new Article
                {
                    Title = "Mobile Arena: Darvianto \"龍珠Easy\" Anthony Ansen Menjadi Midlaner Bigetron eSports",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 7, 0, 0, 0),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/easy-announcement.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/players/easy-announcement.jpg\"><br><br>" +
                        "<p>Setelah kehadiran RoboX, Age dan SiMontok. Bigetron kembali mendatangkan satu pemain guna mengisi posisi Midlaner. Dia bernama Frankie \"龍珠Easy\" Anthony Ansen. 龍珠Easy merupakan salah satu pemain League of Legends (LoL) Indonesia. 龍珠Easy akan membagikan sedikit alasan kenapa dia memilih bermain Mobile Arena.</p><br><br>" +
                        "<p>P: Penulis; E: Frankie \"龍珠Easy\" Anthony Ansen</p><hr>" +
                        "<p>P: Role apa yang akan kamu isi di Bigetron nanti?</p>" +
                        "<p>E: Saya akan mengisi posisi Midlaner/carry di Bigetron. Di ranked, saya juga sering mengisi posisi Midlaner ketika sedang solo queue.</p><br>" +
                        "<p>P: Sudah berapa lama kamu bermain Mobile Arena?</p>" +
                        "<p>E: Kebetulan saya sudah bermain sejak pertama kali dirilis. </p><br>" +
                        "<p>P: Apa yang membuat kamu bermain Mobile Arena?</p>" +
                        "<p>E: Karena Mobile Arena lebih mirip ke LoL untuk gameplaynya dibanding game-game moba lainnya. Mungkin karena sama-sama dari Garena. Selain itu, saya juga adalah pemain LoL Indonesia. Makanya, saya tertarik dengan Mobile Arena.</p><br>" +
                        "<p>P: Apa rencana anda bersama Bigetron?</p>" +
                        "<p>E: Saya ingin membawa Bigetron menjadi salah satu top tim di Indonesia. </p><br>" +
                        "<p>P: Seberapa dekat kamu dengan pemain-pemain Bigetron lainnya?</p>" +
                        "<p>E: Saya dekat dengan semuanya. Kita sering bermain dan berlatih bersama.</p><br>" +
                        "<p>P: Ada yang ingin kamu sampaikan kepada para pembaca?</p>" +
                        "<p>E: Jangan lupa untuk dukung dan support Bigetron terutama kita nanti di Balai Sarbini ya, tanggal 29 Juli 2017 nanti.</p><hr><br>" +
                        "<p>Itulah diskusi singkat bersama 龍珠Easy yang menjadi Midlaner Bigetron. Tunggu info kami selanjutnya yah untuk pemain akhir. See ya guys!</p>"
                },
                new Article
                {
                    Title = "Mobile Arena: Kelvin \"vieL\" Lee Adalah Permain Kelima Bigetron eSports",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 7, 0, 0, 1),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/players/viel-announcement.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/players/viel-announcement.jpg\"><br><br>" +
                        "<p>Kelvin \"vieL\" Lee akan menjadi pemain kelima sekaligus melengkapi tim Bigetron. vieL saat ini telah mencapai tier Grand Conqueror dan menduduki peringkat kelima. Bigetron akan mengikuti pertandingan kompetitif Mobile Arena pertama mereka di Balai Sarbini pada tanggal 29 Juli 2017. Sebagai pemain terakhir, vieL mengutarakan alasan ketertarikannya bermain Mobile Arena dan kedekatannya dengan beberapa pemain.</p><br><br>" +
                        "<p>P: Penulis; V: Kelvin \"vieL\" Lee</p><hr>" +
                        "<p>P: Kamu merupakan pemain terakhir yang bergabung bersama Bigetron. Bagaimana perasaan kamu?</p>" +
                        "<p>V: Senang sekali dapat bergabung bersama Bigetron serta bisa mengenal lebih jauh beberapa teman yang sering main bersama.</p><br>" +
                        "<p>P: Posisi apa yang akan kamu isi di Bigetron?</p>" +
                        "<p>V: Saya akan mengisi posisi Top Tank. Jadi, saya akan memainkan hero-hero dengan role tank</p><br>" +
                        "<p>P: Sejak kapan kamu bermain Mobile Arena?</p>" +
                        "<p>V: Sejak pertama kali release, saya sudah bermain Mobile Arena. </p><br>" +
                        "<p>P: Apa yang mendorong kamu untuk memilih bermain Mobile Arena dibandingkan sama game-game sejenis?</p>" +
                        "<p>V: Saya lebih suka bermain dengan grafik di Mobile Arena daripada game Moba lainnya karena gameplay Mobile Arena jauh lebih seru terus banyak pemain Indo yang main Mobile Arena. Jadinya, kalau kita pakai voice chat lebih nyambung dan mudah dimengerti.</p><br>" +
                        "<p>P: Apa rencana kedepan kamu bersama Bigetron?</p>" +
                        "<p>V: Rencana saya sih, ingin menjadikan Bigetron sebagai tim terbaik dari yang terbaik dalam skala nasional atau internasional.</p><br>" +
                        "<p>P: Siapa teman partner duo kamu ketika sedang queue?</p>" +
                        "<p>V: Saya paling dekat sama Age dan SiMontok. Karena saya paling sering bermain bersama mereka dan kita tiap hari main sama mereka.</p><br>" +
                        "<p>P: Apa yang ingin kamu sampaikan kepada para pembaca Bigetron?</p>" +
                        "<p>V: Ya, keep support Bigetron di Balai Sarbini nanti dan let\'s fly higher together.</p><hr><br>" +
                        "<p>Itulah diskusi singkat bersama vieL yang menjadi Tanker Bigetron. Jangan lupa dukung Bigetron di Balai Sarbini tanggal 29 Juli 2017 ya!</p>"
                },
                new Article
                {
                    Title = "LoL: Stevanus \"Venus\" Budiman Keluar Dari Bigetron eSports",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 9),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/venus-farewell/venus.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/venus-farewell/venus-with-team.jpg\"><br><br>" +
                        "<p>Stevanus \"Venus\" Budiman yang sempat digantikan oleh Bintang \"Binx\" Pamungkas sejak week 3. Hal ini terjadi karena Venus yang berhalangan untuk tampil di League of Legends Garuda Series (LGS). Performa Venus di LGS tidaklah buruk karena dua pertandingan awal, Bigetron harus melawan tim-tim tangguh seperti NXL dan Phoenix eSports. Venus akan menceritakan suka dukanya dalam menjalani dunia kompetitif di LGS.</p><br><br>" +
                        "<p>P: Penulis; V: Stevanus \"Venus\" Budiman</p><hr>" +
                        "<p>P: Sejak kapan kamu bergabung di Bigetron?</p>" +
                        "<p>V: Saya bergabung di Bigetron pada saat sebelum lgs di mulai yaitu LGS Promotion.</p><br>" +
                        "<p>P: Siapa teman dekat kamu di Bigetron?</p>" +
                        "<p>V: Semuanya saya anggap teman dekat saya karena mereka perhatian dan baik sama saya.</p><br>" +
                        "<p>P: Kamu bermain selama 2 minggu bersama Bigetron. Bagaimana kamu melihat peta kekuatan LGS saat ini?</p>" +
                        "<p>V: Menurut saya LGS saat ini seru dan menantang buat saya karena saya juga baru pertama kali bermain di LGS. Bermain di LGS sungguh pengalaman baru dan menyenangkan bagi saya.</p><br>" +
                        "<p>P: Seberapa besar kesempatan Bigetron di LGS season ini?</p>" +
                        "<p>V: Kesempatannya saya rasa sama besar karena semua tim masih dapat memberikan surprise bagi tim-tim yang diatas. Untuk Bigetron, saya berharap mereka mendapatkan prestasi terbaik.</p><br>" +
                        "<p>P: Kamu sudah bermain selama 2 minggu di LGS. Apa alasan kamu untuk keluar dari Bigetron?</p>" +
                        "<p>V: Faktor keluarga menjadi kendala utama bagi saya. Orang tua saya yang kurang menyetujui saya bergelut di dunia game menyebabkan saya tidak dapat melanjutkan bersama Bigetron untuk commit fulltime.</p><br>" +
                        "<p>P: Apa suka dukanya menjadi pemain LGS sendiri apa menurut kamu?</p>" +
                        "<p>V: Sukanya saya ada tantangan lebih di bidang eSports dan mendapat banyak dapet temen baru juga, kalau dukanya saya harus membagi waktu untuk kuliah, latihan, dan membantu keluarga saya yang di Bengkulu.</p><br>" +
                        "<p>P: Menurut kamu, owner Bigetron itu orang yang seperti apa? </p>" +
                        "<p>V: Owner Bigetron sangat saya kagumi, orangnya baik, ramah dan friendly. Dia sangat perhatian ke para pemain. Pokoknya perfect lah sebagai owner.</p><br>" +
                        "<p>P: Apa ada kemungkinan kamu akan kembali ke panggung kompetitif?</p>" +
                        "<p>V: Jujur, untuk saat ini saya belum kepikiran tapi tidak menutup kemungkinan juga saya akan kembali.</p><br>" +
                        "<p>P: Ada pesan yang ingin kamu sampaikan kepada teman-teman di Bigetron?</p>" +
                        "<p>V: Untuk teman-teman di Bigetron, sukses terus ya. Semoga kalian bisa mencapai apa yang sudah kalian targetkan di awal musim, goodluck teman-teman.</p><hr><br>" +
                        "<p>Thank you and good luck kedepannya Venus!</p>"
                },
                new Article
                {
                    Title = "Announcement: Eveline \"Xyera\" Naomi Menjadi Community Manager Bigetron eSports",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 10),
                    CoverImageUrl =
                        "https://s3-ap-southeast-1.amazonaws.com/bigetron/xyera-announcement/xyera-cover.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/xyera-announcement/xyera-cover.jpg" +
                        "\"><br><br>" +
                        "<p>Eveline \"Xyera\" Naomi atau yang biasa dipanggil dengan nama Epe, bukanlah wajah baru di dunia eSports. Epe sudah cukup lama berkecimpung di dunia eSports. Di Bigetron eSports, Epe akan menjabat sebagai Community Manager. Apa tugasnya? Lalu apa tantangannya menjadi seorang Community Manager? Pada kesempatan kali ini, Epe akan memberikan jawaban tentang tugas dan tantangan dari seorang Community Manager.</p><br><br>" +
                        "<p>P: Penulis; X: Eveline \"Xyera\" Naomi</p><hr>" +
                        "<p class=\"font-weight-bold\">P: Eveline \"Xyera\" Naomi, kamu akan menjadi Community Manager di Bigetron. Dapat cerita sedikit tentang latar belakang kamu di dunia eSports?</p>" +
                        "<p>X: Dulu saya pernah bekerja sebagai pengurus komunitas. Pekerjaannya lebih fokus ke mengurus acara planning event baik internal atau ke publik. Lalu saya juga bertugas menjaga hubungan baik antar sesama anggota komunitas.</p><br>" +
                        "<p class=\"font-weight-bold\">P: Apa kamu memiliki pengalaman lain di dunia eSports?</p>" +
                        "<p>X: Saya juga menjadi seorang youtuber gaming. Saya biasanya membawakan konten League of Legends. Tapi rencananya, saya mau menambah game lain, salah satunya Mobile Arena.</p><br>" +
                        "<p class=\"font-weight-bold\">P: Apa alasan kamu untuk masuk ke Bigetron?</p>" +
                        "<p>X: Kebetulan saya memiliki kesamaan visi dan misi dengan owner Bigetron sehingga ketika saya mendapatkan tawaran, saya menerima tawaran tersebut. Semoga saya dapat memberikan yang terbaik bagi Bigetron.</p><br>" +
                        "<p class=\"font-weight-bold\">P: Setuju, visi dan misi yang sama akan membantu memajukan Bigetron. Lalu, apa tugas kamu sebagai Community Manager di Bigetron?</p>" +
                        "<p>X: Tugas saya sebagai Community Manager secara garis besar adalah memperkenalkan tim Mobile Arena dan League of Legends Bigetron kepada publik. Nantinya, saya dan rekan-rekan juga akan mengadakan sesi bermain bersama para pemain Bigetron baik di Mobile Arena atau League of Legends. Kemudian, kita akan membuat event-event menarik dan memajukan tim Bigetron di dunia industri eSports.</p><br>" +
                        "<p class=\"font-weight-bold\">P: Apa tantangan yang akan kamu hadapi untuk menjalankan tugas-tugas ini?</p>" +
                        "<p>X: Tantangannya yang pasti saya harus beradaptasi lagi karena ini merupakan pengalaman baru bagi saya. Lalu, saya mesti mengenal orang-orang yang baru lagi karena Mobile Arena merupakan game mobile terbaru dari Garena. Jadinya, saya akan berusaha lebih cepat membaur dengan komunitasnya juga. Karena mau bagaimanapun, komunitas game mobile dengan komunitas game PC jelas berbeda.</p><br>" +
                        "<p class=\"font-weight-bold\">P: Wah menarik sekali diskusinya, terima kasih atas waktunya. Selamat beraktivitas kembali.</p>" +
                        "<p>X: Ok, terima kasih juga.</p><hr><br>" +
                        "<p>Itulah wawancara singkat bersama Epe. Semoga Epe dapat menjalankan tugasnya di Bigetron eSports dengan baik.</p>"
                },
                new Article
                {
                    Title = "LoL: Bigetron eSports Resmi Datangkan Dua Pemain Vietnam",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 21),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/news-default.jpg",
                    Content =
                        "<p>Bigetron eSports secara resmi telah mendapatkan pemain baru untuk menggantikan Starlest untuk di babak playoffs nanti. Seperti yang kita ketahui bersama, Starlest akan segera pensiun dari dunia kompetitif dan berfokus mengembangkan Bigetron dari belakang layar.</p>" +
                        "<p>Pemain baru tersebut bernama Bui \"Zeref\" Kiet Anh Van di posisi Jungler dan Chau \"Kado\" Nhat Tien yang akan mengisi posisi Top. Kedua pemain ini telah dipantau oleh tim scouting Bigetron. Tim scouting Bigetron telah melakukan scouting di Vietnam College Players selama tiga hari. Zeref dan Kado adalah pemain tier Challenger yang telah bertanding di Vietnam Scene. Scouting selama tiga hari memunculkan nama Zeref dan Kado sebagai yang terbaik pada posisinya masing-masing.</p>" +
                        "<p>Jangan lewatkan debut pertama Zeref dan Kado bersama Bigetron eSports pada playoffs nanti.</p><br><br>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/vietnam-players-announcement/zeref.jpg\">" +
                        "<img class=\"img-fluid mx-auto d-block my-5\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/vietnam-players-announcement/kado.jpg\">"
                },
                new Article
                {
                    Title = "Mobile Arena: Montok Out, Azerith In",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 22),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/news-default.jpg",
                    Content =
                        "<p>Bigetron eSports kini memperbarui roster terbaru mereka di divisi Mobile Arena. Wahyu \"SiMontok\" Febri yang sebelumnya membela Bigetron kini telah berpindah ke tim Western Inifinite. Untuk mengisi posisi yang kosong, Bigetron telah bergerak cepat dan melakukan scouting pada beberapa pemain yang berada di tier tinggi. Bigetron akhirnya memutuskan untuk mendatangkan salah satu pemain di tier Grand Conqueror. Pemain baru tersebut bernama Azerith.</p>" +
                        "<p>Azerith dikenal dengan nama Zulfikar \"Azerith\" akan mengisi posisi yang ditinggalkan SiMontok, yaitu sebagai Assassin. Butterfly telah menjadi champion andalannya selama bermain Mobile Arena. Azerith mulai terjun ke dunia esports sejak bermain League of Legends di tahun 2013. Selain itu, Azerith juga pernah bermain Mobile Legends dan sekarang bermain Mobile Arena sejak open beta dimulai. Hingga saat ini, Azerith telah berada di tier Grand Conqueror dan mencapai rank 2. </p>" +
                        "<p>Jangan lewatkan debut perdana Azerith bersama rekan-rekan barunya nanti di Balai Sarbini pada tanggal 29 Juli 2017.</p><br><br>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/azerith-announcement/rank.jpg\">"
                },
                new Article
                {
                    Title = "LoL: Bigetron eSports Melaju ke Babak Quarterfinals",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 7, 28),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/news-default.jpg",
                    Content =
                        "<p>Duo pemain Vietnam, BTR Kado dan BTR Zeref tampil sangat baik untuk memenangkan pertandingan melawan Kamikaze.</p>" +
                        "<p class=\"font-weight-bold\">Game Pertama</p>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/btr-vs-kz-playoffs/game1-pick-ban.jpg\"><br>" +
                        "<p>Bigetron unggul dengan firstblood yang diperoleh Zeref setelah javelin miliknya berhasil mengenai Jax lawan. Kamikaze membalikkan keadaan dan memiliki keunggulan di menit-menit awal. Bigetron tertinggal dengan skor 2-6 pada menit 10. Namun, sedikit demi sedikit Bigetron membalikkan keadaan dengan sinergi baik dari Zeref dan Kado. KZ Bravo berhasil diculik dan Bigetron dapat mengambil turret di mid lane. Dengan bantuan Rift Herald, Bigetron mampu mengambil turret dan inhibitor Kamikaze  di mid lane sangat mudah. Kamikaze hampir tidak mampu menjawab perlawanan dari Bigetron dan game pertama menjadi milik Bigetron. BTR Kado keluar sebagai MVP di game pertama dengan 500 points.</p>" +
                        "<p class=\"font-weight-bold\">Game Kedua</p>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/btr-vs-kz-playoffs/game2-pick-ban.png\"><br>" +
                        "<p>Sama seperti game pertama, Kamikaze berhasil mempunyai keunggulan dari Bigetron. Nidalee milik Zeref yang terlalu agresif berhasil memberikan firstblood bagi Kamikaze. Tapi keunggulan tersebut tidak bertahan lama, Bigetron kembali membalikkan keadaan. Performa BTR Kado yang sangat luar biasa, BTR Kado dapat melakukan solo kill terhadap KZ Chaser. Kled milik Kado sangat luar biasa dan menutup permainan dengan skor 11-0-10. Dan lagi, BTR Kado tampil sangat sempurna di game kedua dan mendapatkan MVP keduanya di LGS Season 7.</p>" +
                        "<p>Sungguh debut luar biasa dari BTR Zeref dan Kado. Well, jangan lupa tetap dukung Bigetron yang akan menghadapi Phoenix Allstars, tanggal 29 Juli 2017 pukul 13.00 WIB!</p>" +
                        "<p>#BTRWIN</p>"
                },
                new Article
                {
                    Title = "LoL: Bigetron eSports Harus Mengakui Keunggulan Phoenix Allstars",
                    AuthorId = author_Nierr.Id,
                    Date = new DateTime(2017, 7, 29),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/news-default.jpg",
                    Content =
                        "<p class=\"font-weight-bold\">Game Pertama (BTR 0 v 1 PHX)</p>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/btr-vs-phx-quarterfinals/game1-pick-ban.jpg\"><br>" +
                        "<p>Pertandingan pertama berjalan sangat seru. Bigetron sempat memiliki keunggulan di early game. Lalu disusul dengan kedua tim yang saling membunuh sehingga permainan menjadi lebih berimbang. Namun, petaka datang bagi tim Bigetron. Tercatat, PHX Phoenix mampu melakukan steal Baron sebanyak dua kali dengan sangat baik sekali. Lalu, PHX Pokka juga berhasil mencuri Baron sehingga Phoenix Allstars mampu mengambil tiga kali Baron ketika Bigetron berusaha melakukan kontes pada Baron. Keberhasilan ini langsung dimanfaatkan oleh Phoenix Allstars untuk memenangkan pertandingan pertama.</p>" +
                        "<p class=\"font-weight-bold\">Game Kedua (BTR 0 v 2 PHX)</p>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/btr-vs-phx-quarterfinals/game2-pick-ban.jpg\"><br>" +
                        "<p>Game kedua juga berlangsung menarik. Bigetron mampu mengimbangi perlawanan dari Phoenix Allstars. Namun, beberapa kali percobaan BTR Zeref untuk melakukan ganking dapat digagalkan oleh Phoenix Allstars. Masalah datang bagi Bigetron ketika Cho gath milik Luffy telah menumpuk stacknya sehingga Luffy menjadi sangat sulit dibunuh. Dengan kondisi demikian, Sivir milik PHX Santana dapat melakukan free hit dan sulit untuk disentuh oleh para pemain Bigetron. Bigetron harus menerima kekalahan keduanya di babak Quarterfinals.</p>" +
                        "<p class=\"font-weight-bold\">Game Ketiga (BTR 0 v 3 PHX)</p>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/btr-vs-phx-quarterfinals/game3-pick-ban.jpg\"><br>" +
                        "<p>Bigetron hampir memenangkan pertandingan ketiga ini. Ekko milik Binx bermain sangat cemerlang dengan terus melakukan solo kill kepada pemain Phoenix Allstars. Namun, mental yang sudah menurun serta sinergi yang masih belum baik membuat Bigetron tidak mampu berbuat banyak dan harus mengakui keunggulan dari Phoenix Allstars.</p>" +
                        "<p>Well, langkah kita harus terhenti sampai sini. Tapi, jangan khawatir, kami pasti akan bangkit kembali. Keep support us guys, we will be back more stronger for the next season!</p>"
                },
                new Article
                {
                    Title = "SMITE: Bigetron eSports Akuisisi Tim Archipelago",
                    AuthorId = author_Bigetron.Id,
                    Date = new DateTime(2017, 7, 31),
                    CoverImageUrl = "https://s3-ap-southeast-1.amazonaws.com/bigetron/SMITE-announcement/cover.jpg",
                    Content =
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/SMITE-announcement/team-ranking.jpg\"><br>" +
                        "<p>Bigetron eSports menjadi organisasi pertama di South East Asian yang masuk ke Game SMITE. Hal ini terjadi karena Bigetron eSports telah membeli hak kepemilikan dari tim Archipelago.</p>" +
                        "<p>Game SMITE adalah game MOBA yang berisikan 5v5 pemain dengan menggunakan dewa atau dewi sebagai karakternya yang dikembangkan dan dipublikasikan oleh Hi-Rez. Game ini menggunakan perspektif orang ketiga (3rd perspective), seperti Point Blank yang memberikan nuansa unik dan menarik. SMITE telah dirilis sejak tahun 2014 silam dan menyelenggarakan kejuaraan World Championship sejak tahun 2015 hingga sekarang.</p>" +
                        "<p>Tim Archipelago merupakan salah satu tim yang berasal dari Indonesia dan saat ini tengah bertanding di SMITE Invitational South East Asian. Tim Archipelago berisikan enam orang pemain dengan lima pemain sebagai pemain starter dan satu pemain cadangan. Mereka adalah Lasman \"aLittleLover\" Toni (Riau) sebagai kapten tim, lalu ada Jimy \"JimyMasy\" Masy (Jakarta), Rifqi \"Kirip\" Lazuardi (Jakarta), Erwin \"GeolseuDei\" Kurniawan (Surabaya), Nitin \"DraculaV\" Ganesh (India) serta yang terakhir adalah Ryandi \"TangisanIsabella\" Tsubara (Jakarta). Saat ini, Archipelago sedang berada di peringkat pertama dan berhasil mengamankan slot untuk menuju ke babak selanjutnya.</p>" +
                        "<p>Archipelago akan melanjutkan perjalanan mereka untuk meraih posisi pertama se-Asia Tenggara. Archipelago kembali bertanding di babak Semifinal pada tanggal 2 Agustus 2017 dan dilanjutkan Final di tanggal 3 Agustus 2017. Bagi para Bigetroops dan teman-teman Bigetron dapat menyaksikan secara langsung aksi dari Lasman dan kawan-kawan di HighRezTv @ Twitch.</p><br>" +
                        "<img class=\"img-fluid mx-auto d-block\" src=\"https://s3-ap-southeast-1.amazonaws.com/bigetron/SMITE-announcement/playoffs.jpg\"><br>" +
                        "<p>Ayo berikan dukungan dan semangat kalian bagi tim Bigetron SMITE di playoffs nanti! Kemenangan di babak playoffs ini akan memantapkan posisi mereka guna menuju kualifikasi terakhir untuk World Championship.</p>"
                }
            };
            _dbContext.Articles.AddRange(articles);
            _dbContext.SaveChanges();
        }
        #endregion
    }
}