using System.Threading.Tasks;
using Convey.CQRS.Commands;
using Pacco.Services.Customers.Application.Events;
using Pacco.Services.Customers.Application.Services;
using Pacco.Services.Customers.Core.Exceptions;
using Pacco.Services.Customers.Core.Repositories;

namespace Pacco.Services.Customers.Application.Commands.Handlers
{
    public class CompleteCustomerRegistrationHandler : ICommandHandler<CompleteCustomerRegistration>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMessageBroker _messageBroker;

        public CompleteCustomerRegistrationHandler(ICustomerRepository customerRepository, IMessageBroker messageBroker)
        {
            _customerRepository = customerRepository;
            _messageBroker = messageBroker;
        }

        public async Task HandleAsync(CompleteCustomerRegistration command)
        {
            var customer = await _customerRepository.GetAsync(command.CustomerId);
            if (customer is null)
            {
                throw new CustomerNotFoundException(command.CustomerId);
            }

            if (customer.RegistrationCompleted)
            {
                return;
            }
            
            customer.CompleteRegistration(command.FullName, command.Address);
            await _customerRepository.UpdateAsync(customer);
            await _messageBroker.PublishAsync(new CustomerCreated(command.CustomerId));
        }
    }
}