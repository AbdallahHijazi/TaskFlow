using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Users.Handlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IRepository<User> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService currentUser;

        public UpdateUserCommandHandler(
            IRepository<User> repository,
            IUnitOfWork unitOfWork,
            IImageService imageService, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            this.currentUser = currentUser;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var clientId = currentUser.ClientId ?? throw new TaskFlow.Domain.Exceptions.UnauthorizedException("Your session does not contain a client. Please sign in again.");
                var user = await _repository.GetAll()
                    .Where(u => u.Id == request.Id && u.ClientId == clientId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    throw new NotFoundException("المستخدم", request.Id);

                if (string.IsNullOrWhiteSpace(request.Dto.Name))
                    throw new InvalidOperationException("اسم المستخدم مطلوب");

                user.Name = request.Dto.Name.Trim();
                user.CanAccessAi = request.Dto.CanAccessAi;

                if (request.Dto.Image != null && request.Dto.Image.Length > 0)
                {
                    var imageId = await _imageService.SaveImageAsync(
                        request.Dto.Image,
                        cancellationToken);

                    user.ImageId = imageId;
                }

                _repository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId ?? Guid.Empty,
                    ImageId = user.ImageId,
                    CanAccessAi = user.CanAccessAi,
                    ClientId = user.ClientId,
                    UpdatedAt=user.UpdatedAt,
                    CreatedAt=user.CreatedAt,
                    CreatedById=user.CreatedBy
                };
            }
            catch (TaskFlow.Domain.Exceptions.UnauthorizedException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء تحديث المستخدم. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}
