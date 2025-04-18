using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using News_Models.Extenssions;
using News_Models.Model;
using News_Models.ModelVM;
using News_Models.Repositories;
using News_Web_App.Commponents;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace News_Web_App.Controllers
{

    [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]

    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailService _emailService;
        private readonly UserManager<User> _userManager;
        public AdminController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, EmailService emailService, UserManager<User> userManager)
        {
            _unitOfWork=unitOfWork;
            _webHostEnvironment=webHostEnvironment;
            _emailService=emailService;
            _userManager=userManager;
        }

        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]

        // GET: AdminController
        public async Task<ActionResult> Index(int? Page, string? filter)
        {
            var pageNumber = Page ?? 1;
            var pageSize = 5;

            // جلب البيانات كـ IQueryable لتجنب تحميل جميع البيانات في الذاكرة
            var newsQuery = await _unitOfWork.GetRepository<News>().GetAllAsync();

            // ترتيب البيانات من الأحدث إلى الأقدم
            newsQuery = newsQuery.OrderByDescending(x => x.Created);

            // إذا كان هناك فلتر، قم بتطبيق التصفية
            if (!string.IsNullOrEmpty(filter) && filter == "archived")
            {
                newsQuery = newsQuery.Where(x => x.Status == NewsStatus.Archived);

                // إذا لم يكن هناك فلتر، يتم جلب جميع البيانات بدون باجينيشن
                var allNews = newsQuery.ToPagedList();
                return View(allNews);

            }
            // تطبيق الباجينيشن فقط في حالة وجود فلتر
            var pagedNews = newsQuery.ToPagedList(pageNumber, pageSize);
            return View(pagedNews);
        }


        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]

        // GET: AdminController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
            return View(news);
        }


        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]

        // GET: AdminController/Create
        public async Task<ActionResult> Create()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim!.Value;


            var userReg = await _userManager.FindByIdAsync(userId);
            return View(new News { Author = userReg.Name});
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news)
        {
            if (ModelState.IsValid)
            {
                news.Created = DateTime.Now;
                news.Updated = DateTime.Now;

                if (news.VideoUpload != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "videos");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(news.VideoUpload.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await news.VideoUpload.CopyToAsync(fileStream);
                    }

                    news.VideoFile = "/uploads/videos/" + uniqueFileName; // حفظ الرابط في قاعدة البيانات
                }

                await _unitOfWork.GetRepository<News>().AddAsync(news);
                await _unitOfWork.SaveChangesAsync();

                return Json(new { success = true, redirectUrl = Url.Action("Index") });
            }

            return Json(new { success = false, message = "الرجاء التحقق من صحة البيانات المدخلة" });
        }


        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper}")]

        // GET: AdminController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News collection)
        {
            try
            {
                var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
                if (news != null)
                {
                    news.Status = collection.Status;
                    news.Title = collection.Title;
                    news.Author = collection.Author;
                    news.Description = collection.Description;
                    news.Updated = DateTime.Now;

                    // التحقق مما إذا كان هناك فيديو جديد مرفوع
                    if (collection.VideoUpload != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "videos");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // حذف الفيديو القديم إن وجد
                        if (!string.IsNullOrEmpty(news.VideoFile))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, news.VideoFile.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // حفظ الفيديو الجديد
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(collection.VideoUpload.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await collection.VideoUpload.CopyToAsync(fileStream);
                        }

                        news.VideoFile = "/uploads/videos/" + uniqueFileName; // تحديث مسار الفيديو
                    }

                    await _unitOfWork.GetRepository<News>().UpdateAsync(news);
                    await _unitOfWork.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(collection);
            }
        }


        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]
        [HttpGet]
        public IActionResult GetMessagesAndComments()
        {
            // احضار بيانات الرسائل غير المقروءة من قاعدة البيانات بطريقة غير متزامنة
            var messages = _unitOfWork.GetRepository<Message>()
                                           .GetAllAsync(x => x.Status == MessageStatus.UnRead).Result;
            var comments = _unitOfWork.GetRepository<Comment>().GetAllAsync(x => x.Status == MessageStatus.UnRead).Result;
            messages = messages.OrderByDescending(x => x.Created);
            var allMessage = messages?
                .Select(m => new MassegeVM
                {
                    Id = m.Id,
                    Name = m.Name,
                    Content = m.Content,
                    Created = m.Created.ToUniversalTime().TimeAgo()
                })
                .Take(3)
                .ToList();

            comments = comments.OrderByDescending(x => x.CreatedAt);
            var allComments= comments?
                .Select(m => new CommentVM
                {
                    Id = m.Id,
                    CreatedAt = m.CreatedAt.ToUniversalTime().TimeAgo(),
                    NewsId = m.NewsId,
                    Content = m.Content,
                    Author = m.Author,
                    Reply = m.Reply,
                    Status = m.Status,
                })
                .Take(3)
                .ToList();

            var countMesaage = messages?.Count() ?? 0;
            var countComments = comments?.Count() ?? 0;
            var data = new
            {
                countMesaage,
                countComments,
                allMessage,
                allComments,

            };
            return Ok(data);
        }

        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper}")]


        [HttpDelete]
        public async Task<IActionResult> Remove(int? id)
        {
            var newsItem = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);

            if (newsItem == null)
            {
                return Json(new { success = false, message = "الخبر غير موجود" });
            }
            // حذف الفيديو القديم إن وجد
            if (!string.IsNullOrEmpty(newsItem.VideoFile))
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, newsItem.VideoFile.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
            await _unitOfWork.GetRepository<News>().DeleteAsync(newsItem);
            await _unitOfWork.SaveChangesAsync();
            return Json(new { success = true, message = "تم حذف الخبر بنجاح" });
        }
























        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper}")]

        public async Task<IActionResult> Setting()
        {
            var setting = _unitOfWork.GetRepository<Settings>().GetAllAsync().GetAwaiter().GetResult();
            return View(setting.FirstOrDefault());
        }


        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper}")]

        // GET: AdminController/Edit/5
        public async Task<ActionResult> EditASetting(int id)
        {
            var news = await _unitOfWork.GetRepository<Settings>().GetByIdAsync(id);
            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditASetting(int id, Settings collection)
        {
            try
            {
                var setting = await _unitOfWork.GetRepository<Settings>().GetByIdAsync(id);
                if (setting != null)
                {
                    // تحديث البيانات النصية
                    setting.Title = collection.Title;
                    setting.Facebook = collection.Facebook;
                    setting.TikTok = collection.TikTok;
                    setting.Twitter = collection.Twitter;
                    setting.Instagram = collection.Instagram;
                    setting.AboutUs = collection.AboutUs;
                    setting.Username = collection.Username;
                    setting.Password = collection.Password;
                    setting.FromEmail = collection.FromEmail;
                    setting.FromName = collection.FromName;

                    // تحديث الشعار (Logo)
                    if (collection.LogoFile != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await collection.LogoFile.CopyToAsync(memoryStream);
                            setting.Logo = memoryStream.ToArray(); // تحويل الصورة إلى مصفوفة بايت
                        }
                    }

                    // تحديث الإعدادات في قاعدة البيانات
                    await _unitOfWork.GetRepository<Settings>().UpdateAsync(setting);
                    await _unitOfWork.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Setting));
            }
            catch
            {
                return View(collection);
            }
        }

        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]
        public async Task<IActionResult> Message(int? id, string? filter)
        {
            if (id.HasValue)
            {
                var message = await _unitOfWork.GetRepository<Message>().GetByIdAsync(id.Value);
                if (message == null)
                {
                    return NotFound(); // إرجاع صفحة خطأ إذا لم يتم العثور على الرسالة
                }

                // تحديث الحالة إلى "مقروءة" إذا لم تكن كذلك
                if (message.Status == MessageStatus.UnRead)
                {
                    message.Status = MessageStatus.Read;
                    await _unitOfWork.SaveChangesAsync(); // حفظ التغييرات
                }

                return View("MessageDetails", message); // عرض تفاصيل الرسالة
            }

            // جلب جميع الرسائل كـ IQueryable لتحسين الأداء
            var query = await _unitOfWork.GetRepository<Message>().GetAllAsync();

            if (string.Equals(filter, "Unread", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == MessageStatus.UnRead);
            }
            else if (string.Equals(filter, "Read", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == MessageStatus.Read);
            }
            else if (string.Equals(filter, "Archived", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.IsArchived);
            }
            else if (string.Equals(filter, "UnreadOrArchived", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.Status == MessageStatus.UnRead || x.IsArchived);
            }

           
            return View(query.ToList());
        }



        [Authorize(Roles = $"{RoleConst.AdminRole},{RoleConst.AdminHelper},{RoleConst.Editor}")]

        public async Task<IActionResult> MessageDetails(int? id)
        {
            var message = await _unitOfWork.GetRepository<Message>().GetByIdAsync(id);

            return View(message);
        }

        public async Task<IActionResult> ArchiveNews(int id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id);
            if (news == null)
            {
                return Json(new { success = false, message = "الخبر غير موجود." });
            }
            if (news.Status == NewsStatus.Archived)
            {
                news.Status = NewsStatus.Published; // تعيين حالة الأرشيف

            }
            else
            {
                news.Status = NewsStatus.Archived; // تعيين حالة الأرشيف

            }
            await _unitOfWork.SaveChangesAsync();
            TempData["SuccessMessage"] = "تمت أرشفة الخبر بنجاح!";
            return RedirectToAction(nameof(Index));
        }


       
        [HttpPost]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                // البحث عن الرسالة باستخدام الـ id
                var message = await _unitOfWork.GetRepository<Message>().GetByIdAsync(id);

                if (message == null)
                {
                    return Json(new { success = false, message = "الرسالة غير موجودة." });
                }

                // تحديث حالة الأرشيف
                message.IsArchived = true;

                // حفظ التغييرات في قاعدة البيانات
                await _unitOfWork.SaveChangesAsync();

                // إرجاع رسالة نجاح
                return Json(new { success = true, message = "تم أرشفة الرسالة بنجاح." });
            }
            catch (System.Exception ex)
            {
                // إرجاع رسالة خطأ في حالة حدوث استثناء
                return Json(new { success = false, message = "حدث خطأ أثناء أرشفة الرسالة: " + ex.Message });
            }
        }

        public async Task<IActionResult> MessageEdit(int? id)
        {
            var message = await _unitOfWork.GetRepository<Message>().GetByIdAsync(id);
            return View(message);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MessageEdit(Message collection)
        {
            try
            {
                // التحقق من صحة النموذج
                if (!ModelState.IsValid)
                {
                    return View(collection);
                }

                // استرداد الرسالة من قاعدة البيانات
                var MessageEdit = await _unitOfWork.GetRepository<Message>().GetByIdAsync(collection.Id);
                if (MessageEdit == null)
                {
                    return NotFound(); // إذا لم يتم العثور على الرسالة
                }

                // تحديث خصائص الرسالة
                MessageEdit.Reply = collection.Reply;
                MessageEdit.Content = collection.Content;
                MessageEdit.Email = collection.Email;
                MessageEdit.IsArchived = collection.IsArchived;
                MessageEdit.Status = collection.Status;
                MessageEdit.Name = collection.Name;
                MessageEdit.Updated = DateTime.Now;

                // تحديث الرسالة في قاعدة البيانات
                await _unitOfWork.GetRepository<Message>().UpdateAsync(MessageEdit);
                await _unitOfWork.SaveChangesAsync();

                // إعادة توجيه المستخدم إلى صفحة الفهرس بعد التحديث
                return RedirectToAction(nameof(Message));
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، إرجاع النموذج مع رسالة الخطأ
                ModelState.AddModelError(string.Empty, "An error occurred while updating the message.");
                return View(collection);
            }
        }

        [Authorize(Roles = RoleConst.AdminRole)]
        [Authorize(Roles = RoleConst.Editor)]

        [HttpPost]
        public async Task<IActionResult> Reply(int id, string replyContent)
        {
            try
            {
                // استرداد الرسالة من قاعدة البيانات
                var message = await _unitOfWork.GetRepository<Message>().GetByIdAsync(id);
                if (message == null)
                {
                    return Json(new { success = false, message = "Message not found." });
                }

                // تحديث الرد وحالة الرسالة
                message.Reply = replyContent;
                message.Status = MessageStatus.Read; // تغيير حالة الرسالة إلى "تم الرد"

                // حفظ التغييرات في قاعدة البيانات
                await _unitOfWork.GetRepository<Message>().UpdateAsync(message);
                await _unitOfWork.SaveChangesAsync();

                // إرسال الرد عبر البريد الإلكتروني
                var subject = "Reply to Your Message";
                var body = $"Dear {message.Name},<br><br>Your message has been replied:<br><br>{replyContent}<br><br>Best regards,<br>Your Company";
                await _emailService.SendEmailAsync(message.Email, subject, body);


                return Json(new { success = true, message = "Reply sent successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveMessage(int? id)
        {
            var newsItem = await _unitOfWork.GetRepository<Message>().GetByIdAsync(id);

            if (newsItem == null)
            {
                return Json(new { success = false, message = "الخبر غير موجود" });
            }

            await _unitOfWork.GetRepository<Message>().DeleteAsync(newsItem);
            await _unitOfWork.SaveChangesAsync();
            return Json(new { success = true, message = "تم حذف الخبر بنجاح" });
        }

    }

}
