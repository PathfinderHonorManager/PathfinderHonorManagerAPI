using Microsoft.AspNetCore.Mvc;

namespace PathfinderHonorManager.Controllers
{
    public class ApiController : ControllerBase
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public void UpdateModelState(FluentValidation.ValidationException validationException)
        {
            foreach (var error in validationException.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
