using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Images.Commands;

namespace TaskFlow.API.Controllers.Images;

[Route("api/[controller]")]
[ApiController]
[RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
[RequestSizeLimit(20 * 1024 * 1024)]
public class ImagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "الملف مطلوب" });

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new CreateImageCommand(stream, file.FileName, file.ContentType ?? string.Empty));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllImagesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetImageByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var file = await _mediator.Send(new GetImageFileQuery(id));
        return File(file.Stream, file.ContentType, file.DownloadName, enableRangeProcessing: true);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "الملف مطلوب" });

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UpdateImageCommand(id, stream, file.FileName, file.ContentType ?? string.Empty));
        return Ok(new { Message = "تم تحديث الصورة بنجاح", Data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteImageCommand(id));
        return NoContent();
    }
}
