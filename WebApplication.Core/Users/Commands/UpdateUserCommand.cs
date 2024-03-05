using AutoMapper;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.Core.Common.Exceptions;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        const string NotFoundMessageTemplate = "The user '{0}' could not be found.";
        public int Id { get; set; }
        public string GivenNames { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        public class Validator : AbstractValidator<UpdateUserCommand>
        {


            private readonly IUserService _userService;

            public Validator(IUserService userService)
            {
                _userService = userService;
                // TODO: Create validation rules for UpdateUserCommand so that all properties are required.

                // If you are feeling ambitious, also create a validation rule that ensures the user exists in the database.
                RuleFor(x => x.Id)
                  .Custom((id, context) =>
                  {
                      if (!IsUserExists(id))
                      {
                          context.AddFailure("NotFoundFailure", string.Format(NotFoundMessageTemplate, id));
                      }
                  }).When(x => x.Id > 0);

                RuleFor(x => x.Id).GreaterThan(0);
                RuleFor(x => x.GivenNames)
                   .NotEmpty();

                RuleFor(x => x.LastName)
                    .NotEmpty()
                    .Must((e, x) => e.EmailAddress.IsValidEmailAddress())
                    .WithMessage("Email address is not valid.");

                RuleFor(x => x.MobileNumber)
                    .NotEmpty()
                    .Must((f, x) => f.MobileNumber.IsValidMobileNumber())
                    .WithMessage("Mobile Number is not valid.");
            }

            private bool IsUserExists(int id)
            {
                var user = Task.Run(() => _userService.GetAsync(id)).Result;
                return user != null;
            }
        }
        public class Handler : IRequestHandler<UpdateUserCommand, UserDto>
        {
            /// <inheritdoc />
            private readonly IUserService _userService;
            private readonly IMapper _mapper;

            public Handler(IUserService userService, IMapper mapper)
            {
                _userService = userService;
                _mapper = mapper;
            }
            public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                //throw new NotImplementedException("Implement a way to update the user associated with the provided Id.");

                var user = await _userService.GetAsync(request.Id, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException(string.Format(NotFoundMessageTemplate, request.Id));
                }

                user.GivenNames = request.GivenNames;
                user.LastName = request.LastName;

                if (user.ContactDetail == null)
                {
                    user.ContactDetail = new ContactDetail();
                }

                user.ContactDetail.EmailAddress = request.EmailAddress;
                user.ContactDetail.MobileNumber = request.MobileNumber;

                var updatedUser = await _userService.UpdateAsync(user, cancellationToken);

                return _mapper.Map<UserDto>(updatedUser);

            }
        }
    }
}
