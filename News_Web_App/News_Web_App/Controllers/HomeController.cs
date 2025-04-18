using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using News_Models.Extenssions;
using News_Models.Model;
using News_Models.ModelVM;
using News_Models.Repositories;
using News_Models.UserVM;
using News_Web_App.Commponents;
using News_Web_App.Models;
using X.PagedList.Extensions;

namespace News_Web_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> hubContext;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _unitOfWork=unitOfWork;
            _userManager=userManager;
            this.hubContext=hubContext;
        }

        public async Task<IActionResult> Index(int? Page)
        {
            var pageNumber = Page ?? 1;
            var pageSize = 9;

            // «” —Ã«⁄ «·√Œ»«— «·„‰‘Ê—… Ê — Ì»Â« „‰ «·√ÕœÀ ≈·Ï «·√ﬁœ„
            var newsView = await _unitOfWork.GetRepository<News>()
                .GetAllAsync(x => x.Status == NewsStatus.Published);
            newsView = newsView.OrderByDescending(v => v.Created);
            return View(newsView.ToPagedList(pageNumber, pageSize));
        }
        public async Task<IActionResult> Details(int id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
            if (news == null) return NotFound();

            // Ã·» «·ﬂÊﬂÌ «·Õ«·Ì (·Ê „ÊÃÊœ)
            var viewedNews = Request.Cookies["ViewedNews"];
            var viewedIds = !string.IsNullOrEmpty(viewedNews) ? viewedNews.Split(',').ToList() : new List<string>();

            //  Õﬁﬁ ≈–« ﬂ«‰ «·„” Œœ„ ‘«Âœ Â–« «·Œ»— „‰ ﬁ»·
            if (!viewedIds.Contains(id.ToString()))
            {
                news.ViewCount++;

                await _unitOfWork.GetRepository<News>().UpdateAsync(news);
                await _unitOfWork.SaveChangesAsync();

                // √÷› ID «·Œ»— ··ﬂÊﬂÌ
                viewedIds.Add(id.ToString());
                Response.Cookies.Append("ViewedNews", string.Join(",", viewedIds), new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(7), // «·ﬂÊﬂÌ ÌŸ· 7 √Ì«„
                    IsEssential = true,
                    HttpOnly = false // „„ﬂ‰  Œ·ÌÂ« true ··√„«‰ Õ”» «·Õ«·…
                });
            }

            //  Õ„Ì· «· ⁄·Ìﬁ«  «·„— »ÿ… »«·Œ»—
            news.Comments = _unitOfWork.GetRepository<Comment>()
                .GetAllAsync(c => c.NewsId == id).GetAwaiter().GetResult()
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(news);
        }


        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            news.Likes++;
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, likes = news.Likes });
        }

        [HttpPost]
        public async Task<IActionResult> UnLike(int id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            if (news.Likes > 0)
            {
                news.Likes--;
               await _unitOfWork.SaveChangesAsync();
            }

            return Json(new { success = true, likes = news.Likes });
        }
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentViewModel commentModel)
        {
            if (commentModel == null || !ModelState.IsValid)
            {
                return Json(new { success = false });
            }

            var now = DateTime.Now;

            var commentNew = new Comment
            {
                NewsId = commentModel.NewsId,
                Author = commentModel.Author,
                Content = commentModel.Content,
                CreatedAt = now
            };

            await _unitOfWork.GetRepository<Comment>().AddAsync(commentNew);
            await _unitOfWork.SaveChangesAsync();

            var comments = await _unitOfWork.GetRepository<Comment>().GetAllAsync();

            var commentCount = comments.Count(x => x.NewsId == commentModel.NewsId);
            var comment = new
            {
                Author = commentModel.Author,
                Content = commentModel.Content,
                CreatedAt = now.ToString("dd MMMM yyyy", new CultureInfo("ar-AE")),
                CountComment = commentCount
            };
            await hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
               id = 0,
               name =   commentModel.Author,
               content = commentModel.Content,
               timeAgo = now.TimeAgo(),
               status = "ﬁ«„ »«· ⁄·Ìﬁ ⁄·Ï „‰‘Ê—"
            });
            return Json(new
            {
                success = true,
                comment = comment
            });
        }



        public async Task<IActionResult> Contact()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Message collection)
        {
            if (collection == null)
            {
                ModelState.AddModelError("", "«·»Ì«‰«  «·„—”·… €Ì— ’ÕÌÕ….");
                return View(collection);
            }

            if (!ModelState.IsValid)
            {
                return View(collection);
            }

            try
            {
                collection.Created = DateTime.Now;
                await _unitOfWork.GetRepository<Message>().AddAsync(collection);
                await _unitOfWork.SaveChangesAsync();
                await hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    id = collection.Id,
                    name = collection.Name,
                    content = collection.Content,
                    timeAgo = collection.Created.TimeAgo(),
                    status = "ﬁ«„ »«—”«· —”«·…"
                    // „À·«: „‰– œﬁÌﬁ…
                });
                TempData["SuccessMessage"] = " „ ≈—”«· «·—”«·… »‰Ã«Õ!";
                return View(collection);

            }
            catch (Exception ex)
            {
                //  ”ÃÌ· «·Œÿ√ ··„—«Ã⁄…
                _logger.LogError(ex, "ÕœÀ Œÿ√ √À‰«¡ ≈—”«· «·—”«·….");

                ModelState.AddModelError("", "ÕœÀ Œÿ√ √À‰«¡ ≈—”«· «·—”«·…. Ì—ÃÏ «·„Õ«Ê·… „—… √Œ—Ï.");
                return View(collection);
            }
        }
        public async Task<IActionResult> About()
        {
            var users = _userManager.Users.Where(u => u.ShowInfo == true && u.IsDeleted == false && (u.LockoutEnd == null || u.LockoutEnd < DateTime.Now)).Select(user => new AboutUserVM
            {
                Name = user.Name,
                RoleUSer = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault(),
                About = user.About,
                Facebook = user.Facebook,
                GitHub = user.GitHub,
                Twitter = user.Twitter,
                Instagram =user.Instagram,
                Linkedin = user.Linkedin,
                ImageUrl = user.ImageUrl,
            }).ToList();
            return View(users);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // «Õ÷«— »Ì«‰«  «·≈⁄œ«œ«  „‰ ﬁ«⁄œ… «·»Ì«‰« 
            var detailsSetting = _unitOfWork.GetRepository<Settings>().GetAllAsync().GetAwaiter().GetResult().FirstOrDefault();

            if (detailsSetting?.Logo != null && detailsSetting.Logo.Length > 0)
            {
                ViewData["logo"] = Convert.ToBase64String(detailsSetting.Logo);
            }
            else
            {
                ViewData["logo"] = null; // √Ê ’Ê—… «› —«÷Ì…
            }
            ViewData["titleSite"] = detailsSetting?.Title;
            ViewData["facebook"] = detailsSetting?.Facebook;
            ViewData["instagram"] = detailsSetting?.Instagram;
            ViewData["twitter"] = detailsSetting?.Twitter;
            ViewData["tiktok"] = detailsSetting?.TikTok;
            ViewData["aboutUs"] = detailsSetting?.AboutUs;

            base.OnActionExecuting(context);
        }
    }
}
