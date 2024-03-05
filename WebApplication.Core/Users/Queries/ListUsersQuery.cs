using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.Core.Common.Models;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Queries
{
    public class ListUsersQuery : IRequest<PaginatedDto<IEnumerable<UserDto>>>
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; } = 10;

        public class Validator : AbstractValidator<ListUsersQuery>
        {
            public Validator()
            {
                RuleFor(x => x.PageNumber)
                   .GreaterThan(0);
            }
        }

        public class Handler : IRequestHandler<ListUsersQuery, PaginatedDto<IEnumerable<UserDto>>>
        {
            private readonly IUserService _userService;
            private readonly IMapper _mapper;

            /// <inheritdoc />
            public Handler(IUserService userService, IMapper mapper)
            {
                _userService = userService;
                _mapper = mapper;
            }

            public async Task<PaginatedDto<IEnumerable<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
            {
                var users = await _userService.GetPaginatedAsync(request.PageNumber, request.ItemsPerPage, cancellationToken);

                var totalUsers = await _userService.CountAsync(cancellationToken);

                var totalPage = (int)Math.Ceiling((double)totalUsers / request.ItemsPerPage);

                return new PaginatedDto<IEnumerable<UserDto>>() { Data = users.Select(_mapper.Map<UserDto>), HasNextPage = request.PageNumber < totalPage };
            }
        }
    }
}
