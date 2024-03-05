using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using WebApplication.Core.Common.Exceptions;

namespace WebApplication.Core.Common.Behaviours
{
    public class RequestValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        /// <inheritdoc />

        public RequestValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            // TODO: throw a validation exception if there are any validation errors
            // NOTE: the validation exception should contain all failures


            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(validator => validator.Validate(context))
                .SelectMany(validationResult => validationResult.Errors)
                .Where(validationFailure => validationFailure != null)
                .ToList();


            if (!failures.Any()) return await next();
            var notFound = failures.FirstOrDefault(x => x.PropertyName.Equals("NotFoundFailure"));
            if (notFound != null)
            {
                throw new NotFoundException(notFound.ErrorMessage);
            }

            throw new ValidationException(failures);
        }
    }
}
