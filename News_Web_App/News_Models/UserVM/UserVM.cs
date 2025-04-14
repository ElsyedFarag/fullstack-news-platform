using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.UserVM
{
    public class UserVM
    {
        [Display(Name = "المعرف")]
        public string Id { get; set; }

        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "العنوان")]
        public string Address { get; set; }

        [Display(Name = "رقم الهاتف")]
        [Required(ErrorMessage = "رقم الهاتف مطلوب.")]
        [RegularExpression(@"^(\d{10,15})$", ErrorMessage = "الرجاء إدخال رقم هاتف صحيح.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Display(Name = "الأدوار في النظام")]
        [ValidateNever]
        public IEnumerable<string> Roles { get; set; }

        [Display(Name = "الصورة")]
        public byte[]? Image { get; set; }

        public string? Lock { get; set; }
        public bool ShowInfo { get; set; }

    }
}