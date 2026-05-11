using Application.DTOs.Customers;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Application service for Customer CRD operations.
/// Customer update is excluded per business rules.
/// </summary>
public class CustomerAppService : ICustomerAppService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<CreateCustomerDto> _createValidator;
    private readonly ILogger<CustomerAppService> _logger;

    public CustomerAppService(
        ICustomerRepository customerRepository,
        IValidator<CreateCustomerDto> createValidator,
        ILogger<CustomerAppService> logger)
    {
        _customerRepository = customerRepository;
        _createValidator = createValidator;
        _logger = logger;
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new CustomerNotFoundException(id);

        return MapToDto(customer);
    }

    public async Task<List<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdentityNumber = dto.IdentityNumber,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer created. Id: {CustomerId}, Identity: {IdentityNumber}",
            customer.Id, customer.IdentityNumber);

        return MapToDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new CustomerNotFoundException(id);

        // Soft delete
        customer.IsDeleted = true;
        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer soft-deleted. Id: {CustomerId}", id);
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            IdentityNumber = customer.IdentityNumber,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedDate = customer.CreatedDate
        };
    }
}
