using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;
using WebApplication.Pages;

namespace WebApplication.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public partial class AuthorizationFilterAttribute : Attribute, IPageFilter
    {
        protected IndexMode RedirectMode { get; set; } = default;
        public AuthorizationFilterAttribute() : base() { }

        protected virtual IActionResult? HandlerError(string errorMessage, bool validateValue)
        {
            if (validateValue == true) return default(IActionResult);
            return new RedirectToPageResult("Index", new { ErrorMessage = errorMessage, IndexMode = RedirectMode });
        }
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (context.HttpContext.Request.Method != "POST") return;
            Predicate<string?> checkTextField = (string? value) => value != null && value.Length >= 5;

            this.RedirectMode = Enum.Parse<IndexMode>(context.HttpContext.Request.Form["IndexMode"].FirstOrDefault()!);
            IActionResult? validationResult = default;
            var validationList = new Dictionary<string, string?>()
            {
                { "Неверный логин", context.HttpContext.Request.Form["AuthorizationModel.Login"].FirstOrDefault() },
                { "Неверный пароль", context.HttpContext.Request.Form["AuthorizationModel.Password"].FirstOrDefault() },
            };
            if(this.RedirectMode != IndexMode.Authorization)
            {
                validationList.Add("Неверная фамилия", context.HttpContext.Request.Form["AccountModel.Surname"].FirstOrDefault());
                validationList.Add("Неверное имя", context.HttpContext.Request.Form["AccountModel.Name"].FirstOrDefault());
                validationList.Add("Неверное адрес проживания", context.HttpContext.Request.Form["AccountModel.Address"].FirstOrDefault());
            }
            if (this.RedirectMode == IndexMode.Provider)
            {
                validationList.Add("Неверное имя компании", context.HttpContext.Request.Form["ProviderModel.Companyname"].FirstOrDefault());
                validationList.Add("Неверный адрес офиса", context.HttpContext.Request.Form["ProviderModel.Address"].FirstOrDefault());
            }
            else if ((IndexMode.Manager | IndexMode.Builder | IndexMode.Delivery).HasFlag(this.RedirectMode))
            {
                validationList.Add("Ошибка с образованием", context.HttpContext.Request.Form["EmployeeModel.Education"].FirstOrDefault());
            }
            foreach (KeyValuePair<string, string?> record in validationList)
            {
                if ((validationResult = this.HandlerError(record.Key, checkTextField(record.Value))) != null)
                { context.Result = validationResult; return; }
            }
            var phoneValue = context.HttpContext.Request.Form["AccountModel.Phonenumber"].FirstOrDefault();
            var emailValue = context.HttpContext.Request.Form["ClientModel.Emailaddress"].FirstOrDefault();
            var passportValue = context.HttpContext.Request.Form["EmployeeModel.Passport"].FirstOrDefault();

            var validatePhone = phoneValue == null || Regex.IsMatch(phoneValue, @"^\+7[0-9]{10}$");
            if ((validationResult = this.HandlerError("Неправильный формат номера телефона", validatePhone)) != null)
            { context.Result = validationResult; return; }

            var validateEmail = emailValue == null || Regex.IsMatch(emailValue, @"^\w{6,}@(mail|gmail|yandex){1}.(ru|com){1}$");
            if ((validationResult = this.HandlerError("Неправильный формат email", validateEmail)) != null)
            { context.Result = validationResult; return; }

            var validatePassport = passportValue == null || Regex.IsMatch(passportValue, @"^[0-9]{4}-[0-9]{6}$");
            if ((validationResult = this.HandlerError("Ошибка паспорта", validateEmail)) != null)
            { context.Result = validationResult; return; }
        }
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    }
}
