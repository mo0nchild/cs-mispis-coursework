using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApplication.Filters
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method)]
    public partial class ProfileFilterAttribute : Attribute, IPageFilter
    {
        protected string FailedRedirect { get; private set; } = default!;
        public string? MethodValidate { get; set; } = default;
        public ProfileFilterAttribute(string failureLink) : base()  { this.FailedRedirect = failureLink; }

        protected virtual IActionResult? HandlerError(string errorMessage, bool validateValue)
        {
            if (validateValue == true) return default(IActionResult);
            return new RedirectToPageResult(this.FailedRedirect, new { ErrorMessage = errorMessage });
        }
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var handler = context.HttpContext.Request.Query["handler"].FirstOrDefault();
            if (handler != this.MethodValidate || context.HttpContext.Request.Method != "POST") return;

            Predicate<string?> checkTextField = (string? value) => value != null && value.Length >= 5;

            IActionResult? validationResult = default;
            var validationList = new Dictionary<string, string?>()
            {
                { "Неверная фамилия", context.HttpContext.Request.Form["Surname"].FirstOrDefault() },
                { "Неверное имя", context.HttpContext.Request.Form["Name"].FirstOrDefault() },

                { "Неверное адрес проживания", context.HttpContext.Request.Form["Address"].FirstOrDefault() },
            };
            foreach(var record in validationList)
            {
                if ((validationResult = this.HandlerError(record.Key, checkTextField(record.Value))) != null)
                { context.Result = validationResult; return; }
            }
            var phoneValue = context.HttpContext.Request.Form["Phonenumber"].FirstOrDefault();
            var validatePhone = phoneValue == null || Regex.IsMatch(phoneValue, @"^\+7[0-9]{10}$");

            if ((validationResult = this.HandlerError("Неправильный формат номера телефона", validatePhone)) != null)
            { context.Result = validationResult; return; }
        }
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
        public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    }
}
