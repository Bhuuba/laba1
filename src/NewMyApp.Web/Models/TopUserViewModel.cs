using NewMyApp.Core.Models;

namespace NewMyApp.Web.Models
{
    public class TopUserViewModel
    {
        public User User { get; set; }
        public int TotalLikes { get; set; }
        public int Rank { get; set; }
    }
}