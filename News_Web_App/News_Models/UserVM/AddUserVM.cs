using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Models.UserVM
{
    public class AddUserVM
    {
        // الاسم
        [Required(ErrorMessage = "الاسم مطلوب.")]
        [MaxLength(50, ErrorMessage = "الاسم يجب ألا يتجاوز 50 حرفًا.")]
        public string Name { get; set; }

        // الجنس
        [Required(ErrorMessage = "الجنس مطلوب.")]
        [MaxLength(7, ErrorMessage = "الجنس يجب ألا يتجاوز 7 أحرف.")]
        public string Gender { get; set; }

        // تاريخ الميلاد
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly? BirthDate { get; set; }

        // رقم الهاتف
        [Display(Name = "رقم الهاتف")]
        [Required(ErrorMessage = "رقم الهاتف مطلوب.")]
        [RegularExpression(@"^(\d{10,15})$", ErrorMessage = "الرجاء إدخال رقم هاتف صحيح.")]
        public string PhoneNumber { get; set; }

        // البريد الإلكتروني
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "الرجاء إدخال بريد إلكتروني صحيح.")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        // كلمة المرور
        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون بين {2} و {1} حرفًا.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        // تأكيد كلمة المرور
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }

        // الأدوار في النظام
        [Display(Name = "الأدوار في النظام")]
        [ValidateNever]
        public IEnumerable<string> Roles { get; set; }

        // تاريخ الإنشاء
        public DateOnly? Created { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}