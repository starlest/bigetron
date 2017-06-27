using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bigetron.ViewModels
{
    public class CustomerQueryVM
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Subject { get; set; }
        public string Query { get; set; }
        public string Captcha { get; set; }
    }
}
