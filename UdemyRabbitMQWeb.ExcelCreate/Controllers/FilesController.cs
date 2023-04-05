using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Threading.Tasks;
using System;
using UdemyRabbitMQWeb.ExcelCreate.Models;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQWeb.ExcelCreate.Hubs;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext context ,IHubContext<MyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest();


            var userFile = await _context.userFiles.FirstAsync(x => x.Id == fileId);

            var filePath = userFile.FileName + Path.GetExtension(file.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);



            using FileStream stream = new(path, FileMode.Create);

            await file.CopyToAsync(stream);


            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;

            await _context.SaveChangesAsync();
            //SignalR notification oluşturulacak
            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");



            return Ok();
        }
    }
}
