using Nexus.CustomerOrder.Domain.Features.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.CustomerOrder.Application.Features.Accounts;

public record UpdateAccountCommand(
    string AccountId, 
    string FirstName, 
    string LastName, 
    string Email, 
    string Phone, 
    Address Address);

internal class UpdateAccountHandler
{
}
