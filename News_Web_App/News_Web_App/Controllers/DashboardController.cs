using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using News_Models.Model;
using News_Models.Models;
using News_Models.Repositories;
using News_Web_App.Commponents;

namespace News_Web_App.Controllers
{
    [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork=unitOfWork;
        }

        // GET: DashboardController
        public async Task<ActionResult> Index()
        {
            var infoNews = await _unitOfWork.GetRepository<News>().GetAllAsync();
            var infoUsers = await _unitOfWork.GetRepository<User>().GetAllAsync(x=>x.IsDeleted == false);
            var infoMessage = await _unitOfWork.GetRepository<Message>().GetAllAsync();
            // عرض عدد الزوار الفريدين
            var uniqueVisitors = await _unitOfWork.GetRepository<Visitor>().GetAllAsync(v => v.IsUnique);
            ViewData["UniqueVisitors"] = uniqueVisitors.Count();



            ViewData["AllNews"]=infoNews.Count();
            ViewData["ArchNews"]=infoNews.Count(x=>x.Status == NewsStatus.Archived);

            ViewData["AllMessge"]=infoMessage.Count();
            ViewData["Read"]=infoMessage.Count(x => x.Status == MessageStatus.Read);
            ViewData["UnRead"]=infoMessage.Count(x => x.Status == MessageStatus.UnRead);
            ViewData["Archive"]=infoMessage.Count(x => x.IsArchived);

            ViewData["Users"]= (infoUsers.Count());

            return View();
        }
   
    }
}
