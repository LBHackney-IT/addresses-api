using System.Collections.Generic;
using AddressesAPI.V1.Domain;

namespace AddressesAPI.V1.Gateways
{
    public interface IAddressesGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
