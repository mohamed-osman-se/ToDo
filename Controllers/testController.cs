using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class TestController : ControllerBase
{
    public TestController()
    {
    }

    
    [HttpGet]
    public ActionResult<string> test()
    {
        return "Api works!";
    }


}