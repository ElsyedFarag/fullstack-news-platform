// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using News_Models.Model;
using News_Models.Repositories;

namespace News_Web_App.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork=unitOfWork;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        public class InputModel
        {
            /// <summary>
            /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            /// directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public byte[] ProfilePicture { get; set; }

            [Required]
            [MaxLength(50)]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [MaxLength(50)]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [MaxLength(100)]
            [Display(Name = "About Me")]
            public string? About { get; set; }

            [Url]
            [Display(Name = "Facebook Profile")]
            public string? Facebook { get; set; }

            [Url]
            [Display(Name = "GitHub Profile")]
            public string? GitHub { get; set; }

            [Url]
            [Display(Name = "Instagram Profile")]
            public string? Instagram { get; set; }

            [Url]
            [Display(Name = "Twitter Profile")]
            public string? Twitter { get; set; }

            [Url]
            [Display(Name = "LinkedIn Profile")]
            public string? Linkedin { get; set; }
        }


        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ProfilePicture = user.ImageUrl,
                Name = user.Name,
                Address = user.Address,
                About = user.About,
                Linkedin = user.Linkedin,
                Facebook = user.Facebook,
                GitHub = user.GitHub,
                Twitter = user.Twitter,
                Instagram = user.Instagram
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // احضار بيانات الرسائل غير المقروءة من قاعدة البيانات بطريقة غير متزامنة
            var messages = _unitOfWork.GetRepository<Message>()
                                           .GetAllAsync(x => x.Status == MessageStatus.UnRead).Result;

            messages = messages.OrderByDescending(x => x.Created);
            // إعداد بيانات العرض
            ViewData["Message"] = messages?.Take(3).ToList()?? new List<Message>();
            ViewData["MessageCount"] = messages?.Count() ?? 0;


            await LoadAsync(user);
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update Phone Number
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Get current user from DB
            var userUP = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userUP == null)
            {
                return NotFound("User not found.");
            }

            // Update basic info
            if (Input.Name != userUP.Name)
                userUP.Name = Input.Name;

            if (Input.Address != userUP.Address)
                userUP.Address = Input.Address;

            // Update Social Links
            userUP.Facebook = Input.Facebook;
            userUP.GitHub = Input.GitHub;
            userUP.Instagram = Input.Instagram;
            userUP.Twitter = Input.Twitter;
            userUP.Linkedin = Input.Linkedin;
            user.About = Input.About;
            // Save updated user info
            var updateResult = await _userManager.UpdateAsync(userUP);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            // Handle profile picture upload
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files.FirstOrDefault();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (file.Length <= 2097152 && allowedExtensions.Contains(fileExtension))
                {
                    using (var dataStream = new MemoryStream())
                    {
                        await file.CopyToAsync(dataStream);
                        userUP.ImageUrl = dataStream.ToArray();
                    }

                    await _userManager.UpdateAsync(userUP);
                }
                else
                {
                    StatusMessage = "File must be a .jpg or .png image and less than 2MB.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }


    }
}
