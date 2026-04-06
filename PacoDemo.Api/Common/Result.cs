using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace PacoDemo.Api.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result) =>
        result.Match(
            Succ: value => (IActionResult)new OkObjectResult(value),
            Fail: error => error switch
            {
                NotFoundException   => new NotFoundObjectResult(error.Message),
                ConflictException   => new ConflictObjectResult(error.Message),
                BadRequestException => new BadRequestObjectResult(error.Message),
                _                   => new ObjectResult(error.Message) { StatusCode = 500 }
            });
}
