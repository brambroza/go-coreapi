/* using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using coreapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace coreapi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SystemLogController : ControllerBase
    {

      private readonly RabbitMQService _rabbitMQService;

        public SystemLogController(RabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        [HttpPost("[action]")]
        public IActionResult setLogClick([FromBody] LogRequest log)
        {
            _rabbitMQService.SendLog(log: log);
            return Ok();
        }
    }
}
 */
