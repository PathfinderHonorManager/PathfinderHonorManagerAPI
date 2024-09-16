using Microsoft.AspNetCore.Mvc;

namespace PathfinderHonorManager.Controllers
{
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
