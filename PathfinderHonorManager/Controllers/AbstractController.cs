using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace PathfinderHonorManager.Controllers
{
    [ExcludeFromCodeCoverage]
    public abstract class CustomApiController : Controller
    {
        [NonAction]
        protected void UpdateModelState(FluentValidation.ValidationException validationException)
        {
            foreach (var error in validationException.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
